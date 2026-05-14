document.addEventListener('DOMContentLoaded', async () => {
  const params = new URLSearchParams(location.search);
  const id = params.get('id');
  const message = document.querySelector('#user-message');

  if (!id) {
    message.textContent = '缺少用户 ID';
    return;
  }

  try {
    const result = await requestJson(`/api/user/${id}`);
    renderUserHome(result.data);
  } catch (error) {
    message.textContent = error.message;
  }
});

function renderUserHome(data) {
  const user = data.user;
  document.querySelector('#user-avatar').src = user.avatar || '/assets/default-avatar.svg';
  document.querySelector('#user-name').textContent = user.username;
  document.querySelector('#user-email').textContent = user.email;
  document.querySelector('#user-phone').textContent = user.phone || '未填写';
  document.querySelector('#user-created').textContent = new Date(user.createTime).toLocaleDateString('zh-CN');

  const list = document.querySelector('#user-products');
  if (!data.products.length) {
    list.innerHTML = '<div class="col-12"><div class="empty-state">该用户暂未发布商品</div></div>';
    return;
  }

  list.innerHTML = data.products.map(product => `
    <div class="col-md-4">
      <div class="product-summary-card h-100">
        <img src="${product.imageUrl || '/assets/default-product.svg'}" alt="${product.title}">
        <div class="p-3">
          <h3 class="h6 mb-2">${product.title}</h3>
          <div class="text-success fw-bold">¥${Number(product.price).toFixed(2)}</div>
          <div class="small text-secondary mt-1">${product.category}</div>
        </div>
      </div>
    </div>
  `).join('');
}
