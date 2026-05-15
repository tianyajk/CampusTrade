let currentUser = null;
let productId = null;

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
  productId = Number(params.get('id'));

  if (!productId) {
    showMessage('商品参数不正确', false);
    return;
  }

  currentUser = await checkAuth();
  await loadDetail();
});

async function loadDetail() {
  showMessage('正在加载商品详情...', true);

  try {
    const result = await requestJson(`/api/product/${productId}`, { redirectOnUnauthorized: false });
    renderDetail(result.data);
    showMessage('', true);
  } catch (error) {
    showMessage(error.message, false);
  }
}

function renderDetail(data) {
  const product = data.product;
  const seller = data.seller;
  const isSeller = currentUser && currentUser.id === product.sellerId;
  const container = document.querySelector('#product-detail');

  container.classList.remove('d-none');
  container.innerHTML = `
    <div class="row g-4">
      <div class="col-lg-6">
        <img class="product-detail-image" src="${escapeHtml(product.imageUrl || '/assets/default-product.svg')}" alt="${escapeHtml(product.title)}">
      </div>
      <div class="col-lg-6">
        <div class="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <span class="badge text-bg-success mb-2">${escapeHtml(product.category)}</span>
            <h1 class="h3 fw-bold mb-2">${escapeHtml(product.title)}</h1>
            <p class="product-detail-price mb-0">¥${Number(product.price).toFixed(2)}</p>
          </div>
          <a class="btn btn-outline-secondary btn-sm" href="/pages/products.html">返回列表</a>
        </div>

        <hr>
        <h2 class="h5 fw-bold">商品描述</h2>
        <p class="text-secondary product-description">${escapeHtml(product.description)}</p>

        <hr>
        <h2 class="h5 fw-bold">卖家信息</h2>
        <div class="d-flex align-items-center gap-3 mb-4">
          <img class="seller-avatar" src="${escapeHtml(seller.avatar || '/assets/default-avatar.svg')}" alt="卖家头像">
          <div>
            <a class="fw-bold text-success text-decoration-none" href="/pages/user.html?id=${seller.id}">${escapeHtml(seller.username)}</a>
            <p class="text-secondary small mb-0">发布时间：${formatDate(product.createTime)}</p>
          </div>
        </div>

        <div class="d-flex flex-wrap gap-2">
          ${isSeller ? `
            <a class="btn btn-success" href="/pages/publish.html?id=${product.id}">编辑商品</a>
            <button id="delete-button" class="btn btn-outline-danger" type="button">删除商品</button>
          ` : `
            <button class="btn btn-success" type="button" disabled>立即购买（第四阶段开放）</button>
            <button class="btn btn-outline-success" type="button" disabled>收藏（第四阶段开放）</button>
            <button class="btn btn-outline-secondary" type="button" disabled>联系卖家（第四阶段开放）</button>
          `}
        </div>
      </div>
    </div>
  `;

  const deleteButton = document.querySelector('#delete-button');
  if (deleteButton) {
    deleteButton.addEventListener('click', deleteProduct);
  }
}

async function deleteProduct() {
  if (!confirm('确定要删除这个商品吗？')) return;

  try {
    await requestJson(`/api/product/${productId}`, { method: 'DELETE' });
    location.href = '/pages/products.html';
  } catch (error) {
    showMessage(error.message, false);
  }
}

function showMessage(text, success) {
  const message = document.querySelector('#detail-message');
  message.textContent = text;
  message.className = success ? 'form-message text-secondary' : 'form-message text-danger';
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString('zh-CN') : '';
}
