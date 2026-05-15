async function authFetch(url, options = {}) {
  const headers = new Headers(options.headers || {});

  if (options.body && !(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(url, {
    ...options,
    headers,
    credentials: 'same-origin' // 自动带上 Cookie
  });

  if (response.status === 401) {
    if (!location.pathname.endsWith('/login.html')) {
      location.href = '/pages/login.html';
    }
  }

  return response;
}

async function requestJson(url, options = {}) {
  const response = await authFetch(url, options);
  const text = await response.text();
  const payload = text ? JSON.parse(text) : null;

  if (!response.ok) {
    const message = payload?.message || '请求失败，请稍后重试';
    throw new Error(message);
  }

  return payload;
}

async function checkAuth() {
  try {
    const result = await requestJson('/api/user/profile');
    return result.data;
  } catch {
    return null;
  }
}

async function requireAuth() {
  const user = await checkAuth();
  if (!user) {
    location.href = '/pages/login.html';
    return null;
  }
  return user;
}

async function logout() {
  try {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'same-origin' });
  } catch {}
  location.href = '/pages/login.html';
}

async function renderAuthNav() {
  const container = document.querySelector('[data-auth-nav]');
  if (!container) return;

  const user = await checkAuth();
  if (user) {
    container.innerHTML = `
      <a class="nav-link" href="/pages/profile.html">${user.username || '个人中心'}</a>
      <button class="btn btn-sm btn-outline-success" type="button" data-logout>退出登录</button>
    `;
  } else {
    container.innerHTML = `
      <a class="nav-link" href="/pages/login.html">登录</a>
      <a class="btn btn-sm btn-success" href="/pages/register.html">注册</a>
    `;
  }

  const logoutButton = container.querySelector('[data-logout]');
  if (logoutButton) {
    logoutButton.addEventListener('click', logout);
  }
}

document.addEventListener('DOMContentLoaded', renderAuthNav);
