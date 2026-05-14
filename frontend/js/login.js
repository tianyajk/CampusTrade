document.addEventListener('DOMContentLoaded', () => {
  const form = document.querySelector('#login-form');
  const message = document.querySelector('#login-message');

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    message.textContent = '';
    message.className = 'form-message';

    const account = form.account.value.trim();
    const password = form.password.value;

    if (!account || !password) {
      showMessage(message, '请输入账号和密码', false);
      return;
    }

    try {
      const result = await requestJson('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ account, password })
      });

      setToken(result.data.token);
      setStoredUser(result.data.user);
      showMessage(message, '登录成功，正在进入个人中心...', true);
      setTimeout(() => {
        location.href = '/pages/profile.html';
      }, 600);
    } catch (error) {
      showMessage(message, error.message, false);
    }
  });
});

function showMessage(element, text, success) {
  element.textContent = text;
  element.className = success ? 'form-message text-success' : 'form-message text-danger';
}
