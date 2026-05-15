const categories = ['全部', '教材', '电子产品', '生活用品', '服饰', '运动户外', '其他'];

let currentKeyword = '';
let currentCategory = '';
let currentPage = 1;

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

document.addEventListener('DOMContentLoaded', async () => {
  const params = new URLSearchParams(location.search);
  currentKeyword = params.get('keyword') || '';
  currentCategory = params.get('category') || '';
  currentPage = Number(params.get('page') || '1') || 1;

  document.querySelector('#keyword').value = currentKeyword;
  renderCategoryFilters();

  document.querySelector('#search-form').addEventListener('submit', (event) => {
    event.preventDefault();
    currentKeyword = document.querySelector('#keyword').value.trim();
    currentPage = 1;
    navigateWithFilters();
  });

  await loadProducts();
});

async function loadProducts() {
  const message = document.querySelector('#product-message');
  const grid = document.querySelector('#product-grid');
  const pagination = document.querySelector('#pagination');

  message.textContent = '正在加载商品...';
  message.className = 'form-message text-secondary';
  grid.innerHTML = '';
  pagination.innerHTML = '';

  try {
    const params = new URLSearchParams({ page: String(currentPage), pageSize: '12' });
    if (currentKeyword) params.set('keyword', currentKeyword);
    if (currentCategory) params.set('category', currentCategory);

    const result = await requestJson(`/api/product?${params.toString()}`, { redirectOnUnauthorized: false });
    const pageResult = result.data;

    message.textContent = '';
    renderProducts(pageResult.items || []);
    renderPagination(pageResult.page, pageResult.totalPages);
  } catch (error) {
    message.textContent = error.message;
    message.className = 'form-message text-danger';
  }
}

function renderCategoryFilters() {
  const container = document.querySelector('#category-filters');
  container.innerHTML = categories.map((category) => {
    const value = category === '全部' ? '' : category;
    const active = value === currentCategory ? 'active' : '';
    return `<button class="btn btn-sm btn-outline-success category-filter ${active}" type="button" data-category="${escapeHtml(value)}">${escapeHtml(category)}</button>`;
  }).join('');

  container.querySelectorAll('[data-category]').forEach((button) => {
    button.addEventListener('click', () => {
      currentCategory = button.dataset.category || '';
      currentPage = 1;
      navigateWithFilters();
    });
  });
}

function renderProducts(products) {
  const grid = document.querySelector('#product-grid');

  if (!products.length) {
    grid.innerHTML = '<div class="col-12"><div class="empty-state">暂无符合条件的商品</div></div>';
    return;
  }

  grid.innerHTML = products.map((product) => `
    <div class="col-sm-6 col-lg-4 col-xl-3">
      <a class="product-card-link" href="/pages/product-detail.html?id=${product.id}">
        <article class="product-summary-card product-card h-100">
          <img src="${escapeHtml(product.imageUrl || '/assets/default-product.svg')}" alt="${escapeHtml(product.title)}">
          <div class="p-3">
            <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
              <h2 class="h6 fw-bold mb-0 text-dark product-title">${escapeHtml(product.title)}</h2>
              <span class="badge text-bg-success">${escapeHtml(product.category)}</span>
            </div>
            <p class="product-price mb-2">¥${Number(product.price).toFixed(2)}</p>
            <p class="text-secondary small mb-0">卖家：${escapeHtml(product.sellerName || '未知用户')}</p>
          </div>
        </article>
      </a>
    </div>
  `).join('');
}

function renderPagination(page, totalPages) {
  const pagination = document.querySelector('#pagination');
  if (!totalPages || totalPages <= 1) {
    pagination.innerHTML = '';
    return;
  }

  const pages = [];
  for (let index = 1; index <= totalPages; index += 1) {
    pages.push(`
      <li class="page-item ${index === page ? 'active' : ''}">
        <button class="page-link" type="button" data-page="${index}">${index}</button>
      </li>
    `);
  }

  pagination.innerHTML = `
    <li class="page-item ${page <= 1 ? 'disabled' : ''}">
      <button class="page-link" type="button" data-page="${page - 1}">上一页</button>
    </li>
    ${pages.join('')}
    <li class="page-item ${page >= totalPages ? 'disabled' : ''}">
      <button class="page-link" type="button" data-page="${page + 1}">下一页</button>
    </li>
  `;

  pagination.querySelectorAll('[data-page]').forEach((button) => {
    button.addEventListener('click', () => {
      if (button.closest('.disabled')) return;
      currentPage = Number(button.dataset.page);
      navigateWithFilters();
    });
  });
}

function navigateWithFilters() {
  const params = new URLSearchParams();
  if (currentKeyword) params.set('keyword', currentKeyword);
  if (currentCategory) params.set('category', currentCategory);
  if (currentPage > 1) params.set('page', String(currentPage));

  location.href = `/pages/products.html${params.toString() ? `?${params.toString()}` : ''}`;
}
