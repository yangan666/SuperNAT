const Menu = [
  { header: "控制面板" },
  {
    title: "请求统计",
    icon: "dashboard",
    name: "Dashboard"
  },
  {
    title: "用户管理",
    icon: "dashboard",
    name: "User"
  },
  { header: "内网穿透" },
  {
    title: "端口映射",
    icon: "dashboard",
    name: "Map"
  }
]
// reorder menu
Menu.forEach(item => {
  if (item.items) {
    item.items.sort((x, y) => {
      let textA = x.title.toUpperCase()
      let textB = y.title.toUpperCase()
      return textA < textB ? -1 : textA > textB ? 1 : 0
    })
  }
})

export default Menu
