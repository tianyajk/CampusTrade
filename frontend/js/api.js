const TOKEN_KEY = 'campusTradeToken';
const USER_KEY = 'campusTradeUser';

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

function setStoredUser(user) {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function getStoredUser() {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

async function authFetch(url, options = {}) {
  const headers = new Headers(options.headers || {});
  const token = getToken();

  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  if (options.body && !(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(url, {
    ...options,
    headers
  });

  if (response.status === 401) {
    clearToken();
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

function requireAuth() {
  if (!getToken()) {
    location.href = '/pages/login.html';
    return false;
  }

  return true;
}

function logout() {
  clearToken();
  location.href = '/pages/login.html';
}

function renderAuthNav() {
  const container = document.querySelector('[data-auth-nav]');
  if (!container) return;

  const user = getStoredUser();
  if (getToken()) {
    container.innerHTML = `
      <a class="nav-link" href="/pages/profile.html">${user?.username || '个人中心'}</a>
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
