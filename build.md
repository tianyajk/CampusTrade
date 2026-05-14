你是一名资深 C# 全栈开发工程师，现在帮我开发一个"校园二手交易平台"。

# 项目背景

项目名称：
CampusTrade（校园二手交易平台）

项目目标：
开发一个面向大学生的简单校园二手交易系统。

用户可以：
1. 注册登录
2. 发布商品
3. 浏览商品
4. 搜索商品
5. 查看商品详情
6. 收藏商品
7. 与卖家聊天
8. 下单购买
9. 查看个人中心

项目定位：
这是一个课程项目，不要求高并发，不需要复杂分布式架构。
重点是代码规范、项目结构清晰、可运行。

# 技术要求

开发语言：
C#

框架：
ASP.NET Core Web API（后端只提供 API，返回 JSON）

数据库：
MySQL

ORM：
Entity Framework Core

前端：
HTML
CSS
Bootstrap
原生 JavaScript（使用 fetch 调用后端 API）

架构：
前后端分离（同一个项目内，代码分 backend/ 和 frontend/ 目录）

开发工具：
Visual Studio 或 VSCode

# 项目结构

CampusTrade/
├── backend/
│   ├── Controllers/        # API 控制器（返回 JSON）
│   ├── Models/             # 数据库实体 + DTO（请求/响应模型）
│   ├── Services/           # 业务逻辑 + 数据访问（直接注入 DbContext）
│   ├── Data/               # DbContext、数据库迁移
│   └── Program.cs          # 应用入口、服务注册、UseStaticFiles 指向 frontend/
│
├── frontend/
│   ├── pages/              # HTML 页面
│   ├── js/                 # JavaScript（API 调用、页面交互）
│   ├── app.css             # 自定义样式（Bootstrap 之外的补充）
│   └── assets/             # 图片、图标
│
└── CampusTrade.csproj

关键设计说明：
- 后端纯 API，只返回 JSON，无 Razor 视图
- Models/ 同时放实体类和 DTO，不单独拆目录
- 前端纯静态文件，通过 fetch 调用后端 API
- UseStaticFiles 直接指向 frontend/ 目录，不需要 wwwroot
- 登录后 JWT Token 存在 localStorage，每次请求通过 Authorization 头携带
- 不需要 Node.js 构建工具，前端直接写原生 HTML/CSS/JS
- 开发时 API 和前端同域，无需处理跨域

# 功能模块

## 用户模块

实现：
- 用户注册
- 用户登录
- JWT认证
- 修改个人信息
- 上传头像
- 查看个人主页

API 端点设计参考：
POST   /api/auth/register      # 注册
POST   /api/auth/login         # 登录，返回 JWT Token
GET    /api/user/profile       # 获取当前用户信息
PUT    /api/user/profile       # 修改个人信息
POST   /api/user/avatar        # 上传头像
GET    /api/user/{id}          # 查看用户主页

用户实体字段：
User
{
  Id
  Username
  Password（加密存储）
  Email
  Avatar
  Phone
  CreateTime
}

---

## 商品模块

实现：
- 发布商品
- 修改商品
- 删除商品
- 商品列表（分页）
- 商品详情

API 端点设计参考：
GET    /api/product            # 商品列表（支持分页、分类、搜索参数）
GET    /api/product/{id}       # 商品详情
POST   /api/product            # 发布商品（需登录）
PUT    /api/product/{id}       # 修改商品（仅卖家本人）
DELETE /api/product/{id}       # 删除商品（仅卖家本人）

商品实体字段：
Product
{
  Id
  Title
  Description
  Price
  ImageUrl
  Category
  Status（出售中 / 已售出）
  SellerId
  CreateTime
}

---

## 收藏模块

实现：
- 收藏商品
- 取消收藏
- 查看收藏列表

API 端点设计参考：
POST   /api/favorite/{productId}      # 收藏商品
DELETE /api/favorite/{productId}      # 取消收藏
GET    /api/favorite                  # 查看我的收藏列表

---

## 搜索模块

实现：
- 商品名称搜索
- 分类搜索
- 模糊搜索

搜索功能整合在 GET /api/product 中，通过查询参数实现：
/api/product?keyword=教材&category=图书&page=1&pageSize=12

---

## 聊天模块

实现：
用户之间发送消息

API 端点设计参考：
POST /api/message              # 发送消息
GET  /api/message/{userId}     # 查询与某用户的聊天记录
GET  /api/message/contacts     # 获取会话列表

Message 实体字段：
{
  Id
  SenderId
  ReceiverId
  Content
  SendTime
}

聊天功能第一版使用数据库存储消息，前端定时轮询刷新消息即可。

---

## 订单模块

实现：
- 创建订单
- 查看订单
- 确认完成

API 端点设计参考：
POST /api/order                 # 创建订单（购买商品）
GET  /api/order                 # 查看我的订单（买家视角+卖家视角）
PUT  /api/order/{id}/pay        # 卖家确认收款（待付款 → 交易中）
PUT  /api/order/{id}/complete   # 买家确认收货（交易中 → 已完成）

Order 实体字段：
{
  Id
  BuyerId
  ProductId
  Price
  Status（待付款 / 交易中 / 已完成）
  CreateTime
}

交易规则：
- 用户不能购买自己发布的商品
- 订单"已完成"时，对应的商品自动标记为"已售出"
- 卖家可随时下架"出售中"的商品（删除），已售出商品不可删除

---

# 页面清单

前端需要 9 个 HTML 页面：

| 序号 | 页面 | 文件 | 说明 |
|------|------|------|------|
| 1 | 首页 | index.html | 搜索栏、分类导航、商品列表 |
| 2 | 登录页 | login.html | 登录表单 |
| 3 | 注册页 | register.html | 注册表单 |
| 4 | 商品列表页 | product-list.html | 商品卡片网格、筛选、搜索 |
| 5 | 商品详情页 | product-detail.html | 商品信息、卖家信息、收藏/购买 |
| 6 | 发布商品页 | publish.html | 商品信息表单 |
| 7 | 个人中心页 | profile.html | 个人信息、我的发布、我的收藏 |
| 8 | 订单页 | orders.html | 订单列表、状态操作 |
| 9 | 聊天页 | chat.html | 会话列表 + 聊天窗口 |

页面风格：
- 简洁校园风
- 响应式布局
- 使用 Bootstrap

---

# 前端 JavaScript 关键设计

1. API 封装：
在 js/api.js 中封装 fetch 请求，统一处理：
- 基础 URL 配置
- 自动携带 JWT Token（从 localStorage 读取）
- 统一错误处理（401 跳转登录页）
- JSON 解析

2. 认证状态管理：
- 登录成功后 Token 存入 localStorage
- 每个页面加载时检查 localStorage 有无 Token，决定显示"登录"还是"个人中心"
- 退出登录时清除 localStorage

3. 页面渲染：
- 页面加载时通过 fetch 调用 API 获取数据
- 使用原生 DOM 操作或模板字符串动态渲染 HTML
- 表单提交通过 fetch 发送 JSON 数据到 API

---

# 开发规范

要求：
1. 遵循命名规范
   - 类：PascalCase
   - 变量/方法参数：camelCase
   - API 路由：小写 + 连字符

2. 每个方法添加注释

3. 使用依赖注入（Service 统一注册到 DI 容器）

4. 尽量避免重复代码

5. 添加异常处理（每个 API 方法 try-catch）

6. 添加数据验证（前端 JS 校验 + 后端 Data Annotations）

7. 保证代码可读性

---

# 输出要求

开发过程按步骤进行：

第一步：创建项目结构和数据库设计
  - 使用 dotnet new webapi 创建项目
  - 安装 NuGet 包（EF Core MySQL、JWT）
  - 创建 backend/ 下的所有目录
  - 创建 frontend/ 下的所有目录
  - 设计 Entity 类（User, Product, Order, Message, Favorite）
  - 创建 DbContext
  - 配置数据库连接字符串
  - 执行数据库迁移

第二步：完成用户模块
  - AuthController（注册/登录 API + JWT Token 生成）
  - UserController（个人信息、头像上传 API）
  - JWT 认证中间件配置
  - 前端登录页 + 注册页

第三步：完成商品模块 + 搜索模块
  - ProductController（CRUD API + 分页 + 搜索 + 分类筛选）
  - 前端首页 + 商品列表页 + 商品详情页 + 发布商品页

第四步：完成收藏 + 聊天 + 订单模块
  - FavoriteController
  - MessageController
  - OrderController
  - 前端收藏功能、聊天页、订单页

第五步：前端页面整合 + UI 优化
  - 统一导航栏、页脚
  - 响应式布局调试
  - Toast 提示、加载状态
  - 个人中心页面

第六步：测试 + 问题修复 + 交付

每完成一步：
1. 解释实现思路
2. 给出完整代码
3. 告诉我代码放在哪个文件
4. 不要跳步骤
5. 等待我确认再继续

开始第一步。
