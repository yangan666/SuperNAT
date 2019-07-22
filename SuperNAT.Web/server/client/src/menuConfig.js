// 菜单配置
// headerMenuConfig：头部导航配置
// asideMenuConfig：侧边导航配置

const headerMenuConfig = [
  {
    name: '反馈',
    path: 'https://github.com/alibaba/ice',
    external: true,
    newWindow: true,
    icon: 'message',
  },
  {
    name: '帮助',
    path: 'https://alibaba.github.io/ice',
    external: true,
    newWindow: true,
    icon: 'bangzhu',
  },
];

// ICON 配置： https://ice.alibaba-inc.com/component/foundationsymbol chart
const asideMenuConfig = [
  {
    name: '主页',
    path: '/analysis',
    icon: 'home2',
  },
  {
    name: '用户管理',
    path: '/userlist',
    icon: 'home2',
  },
];

export { headerMenuConfig, asideMenuConfig };
