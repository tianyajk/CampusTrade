document.addEventListener('DOMContentLoaded', async () => {
  const user = await requireAuth();
  if (!user) return;

  const profileForm = document.querySelector('#profile-form');
  const avatarForm = document.querySelector('#avatar-form');
  const message = document.querySelector('#profile-message');
  const avatarMessage = document.querySelector('#avatar-message');

  await loadProfile();

  profileForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearMessage(message);

    const username = profileForm.username.value.trim();
    const email = profileForm.email.value.trim();
    const phone = profileForm.phone.value.trim();

    if (!username || !email) {
      showMessage(message, '用户名和邮箱不能为空', false);
      return;
    }

    try {
      const result = await requestJson('/api/user/profile', {
        method: 'PUT',
        body: JSON.stringify({ username, email, phone: phone || null })
      });

      renderAuthNav();
      renderProfile(result.data);
      showMessage(message, '个人信息修改成功', true);
    } catch (error) {
      showMessage(message, error.message, false);
    }
  });

  avatarForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearMessage(avatarMessage);

    const file = avatarForm.file.files[0];
    if (!file) {
      showMessage(avatarMessage, '请选择头像文件', false);
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    try {
      const result = await requestJson('/api/user/avatar', {
        method: 'POST',
        body: formData
      });

      document.querySelector('#avatar-preview').src = result.data.avatar;
      showMessage(avatarMessage, '头像上传成功', true);
    } catch (error) {
      showMessage(avatarMessage, error.message, false);
    }
  });
});

async function loadProfile() {
  const result = await requestJson('/api/user/profile');
  renderProfile(result.data);
}

function renderProfile(user) {
  document.querySelector('#avatar-preview').src = user.avatar || '/assets/default-avatar.svg';
  document.querySelector('#profile-username').textContent = user.username;
  document.querySelector('#profile-email').textContent = user.email;
  document.querySelector('#profile-phone').textContent = user.phone || '未填写';
  document.querySelector('#profile-created').textContent = formatDate(user.createTime);

  const form = document.querySelector('#profile-form');
  form.username.value = user.username;
  form.email.value = user.email;
  form.phone.value = user.phone || '';
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString('zh-CN') : '';
}

function clearMessage(element) {
  element.textContent = '';
  element.className = 'form-message';
}

function showMessage(element, text, success) {
  element.textContent = text;
  element.className = success ? 'form-message text-success' : 'form-message text-danger';
}
