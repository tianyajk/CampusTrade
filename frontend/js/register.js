document.addEventListener('DOMContentLoaded', () => {
  const form = document.querySelector('#register-form');
  const message = document.querySelector('#register-message');

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    message.textContent = '';
    message.className = 'form-message';

    const username = form.username.value.trim();
    const email = form.email.value.trim();
    const phone = form.phone.value.trim();
    const password = form.password.value;
    const confirmPassword = form.confirmPassword.value;

    if (!username || !email || !password) {
      showMessage(message, '请填写用户名、邮箱和密码', false);
      return;
    }

    if (password.length < 6) {
      showMessage(message, '密码至少需要 6 位', false);
      return;
    }

    if (password !== confirmPassword) {
      showMessage(message, '两次输入的密码不一致', false);
      return;
    }

    try {
      await requestJson('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, phone: phone || null, password })
      });

      showMessage(message, '注册成功，正在跳转登录页...', true);
      setTimeout(() => {
        location.href = '/pages/login.html';
      }, 800);
    } catch (error) {
      showMessage(message, error.message, false);
    }
  });
});

function showMessage(element, text, success) {
  element.textContent = text;
  element.className = success ? 'form-message text-success' : 'form-message text-danger';
}
