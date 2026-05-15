let currentUser = null;
let editingProductId = null;
let currentImageUrl = null;

document.addEventListener('DOMContentLoaded', async () => {
  currentUser = await requireAuth();
  if (!currentUser) return;

  const params = new URLSearchParams(location.search);
  editingProductId = Number(params.get('id')) || null;

  if (editingProductId) {
    await loadProductForEdit();
  }

  document.querySelector('#product-form').addEventListener('submit', submitProduct);
  document.querySelector('#imageFile').addEventListener('change', previewImage);
});

async function loadProductForEdit() {
  const message = document.querySelector('#product-form-message');
  try {
    const result = await requestJson(`/api/product/${editingProductId}`);
    const product = result.data.product;

    if (product.sellerId !== currentUser.id) {
      showMessage('只能编辑自己发布的商品', false);
      document.querySelector('#submit-button').disabled = true;
      return;
    }

    document.title = '编辑商品 - CampusTrade';
    document.querySelector('#page-title').textContent = '编辑商品';
    document.querySelector('#submit-button').textContent = '保存修改';

    const form = document.querySelector('#product-form');
    form.elements.title.value = product.title;
    form.elements.description.value = product.description;
    form.elements.price.value = product.price;
    form.elements.category.value = product.category;

    if (product.imageUrl && product.imageUrl !== '/assets/default-product.svg') {
      currentImageUrl = product.imageUrl;
      showPreview(product.imageUrl);
    }

    message.textContent = '';
  } catch (error) {
    showMessage(error.message, false);
  }
}

async function submitProduct(event) {
  event.preventDefault();

  const form = document.querySelector('#product-form');
  const title = form.elements.title.value.trim();
  const description = form.elements.description.value.trim();
  const price = Number(form.elements.price.value);
  const category = form.elements.category.value.trim();

  if (!title || !description || !category || !price || price <= 0) {
    showMessage('请完整填写标题、描述、价格和分类', false);
    return;
  }

  let imageUrl = currentImageUrl || null;

  const fileInput = document.querySelector('#imageFile');
  if (fileInput.files.length > 0) {
    try {
      const formData = new FormData();
      formData.append('file', fileInput.files[0]);
      const uploadResult = await requestJson('/api/product/upload-image', {
        method: 'POST',
        body: formData
      });
      imageUrl = uploadResult.data.imageUrl;
    } catch (error) {
      showMessage(error.message, false);
      return;
    }
  }

  const payload = {
    title,
    description,
    price,
    category,
    imageUrl
  };

  try {
    const result = await requestJson(editingProductId ? `/api/product/${editingProductId}` : '/api/product', {
      method: editingProductId ? 'PUT' : 'POST',
      body: JSON.stringify(payload)
    });

    location.href = `/pages/product-detail.html?id=${result.data.id}`;
  } catch (error) {
    showMessage(error.message, false);
  }
}

function previewImage() {
  const file = document.querySelector('#imageFile').files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (event) => showPreview(event.target.result);
  reader.readAsDataURL(file);
}

function showPreview(source) {
  const img = document.querySelector('#image-preview');
  img.src = source;
  img.classList.remove('d-none');
}

function showMessage(text, success) {
  const message = document.querySelector('#product-form-message');
  message.textContent = text;
  message.className = success ? 'form-message text-success' : 'form-message text-danger';
}
