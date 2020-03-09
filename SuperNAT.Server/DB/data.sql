-- 用户表
DELETE FROM `user`;
INSERT INTO `user` (`user_id`, `user_name`, `password`, `wechat`, `email`, `tel`, `is_disabled`, `is_admin`, `role_id`,`create_time`) VALUES ('dd7e6727968544298e016f7f246f879d', 'admin', '3A03EDF0A917A6A757194FCF4D429A5E', NULL, 'admin@qq.com', NULL, '\0', '1', '2c033d577bea44aa825ffae125c7367e', now());

-- 菜单表
DELETE FROM `menu`;
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES (NULL, 'Home', '/', 'DefaultLayout', '首页', 'home', '1', '\0', '\0', '1c4fd2a0a80b4d44acb54fd7887b7297');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('1c4fd2a0a80b4d44acb54fd7887b7297', 'Dashboard', '/dashboard', 'Dashboard', '请求统计', 'dashboard', '2', '\0', '\0', '5aaa053dcbe1409a99801a5803c060a8');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES (NULL, 'System', '/sys', 'DefaultLayout', '系统管理', 'lock', '3', '\0', '\0', '8ab9d1e4197343dcaf43f77d9eab04fa');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('8ab9d1e4197343dcaf43f77d9eab04fa', 'User', '/user', 'user/UserList', '用户管理', 'dashboard', '7', '\0', '\0', '47292ce0101a430f9a3f72c566306a8f');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('471efa4ffa5145bf8f910395a061b71c', 'Client', '/client', 'client/ClientList', '主机管理', 'dashboard', '9', '\0', '\0', '7966d831e5b9497f9c9da8823117579a');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('471efa4ffa5145bf8f910395a061b71c', 'Map', '/map', 'map/MapList', '端口映射', 'dashboard', '10', '\0', '\0', 'a8e634b4ed914267970352e287057610');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('8ab9d1e4197343dcaf43f77d9eab04fa', 'Menu', '/menu', 'menu/Menu', '菜单管理', 'dashboard', '5', '\0', '\0', '090756bdc3e64549a8c38de968f0bec4');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('8ab9d1e4197343dcaf43f77d9eab04fa', 'Role', '/role', 'role/Role', '角色管理', 'dashboard', '6', '\0', '\0', '4d46153bc5794d36a08b69fc8f979f77');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES (NULL, 'Nat', '/nat', 'DefaultLayout', '内网穿透', 'dashboard', '8', '\0', '\0', '471efa4ffa5145bf8f910395a061b71c');
INSERT INTO `supernat`.`menu` (`pid`, `name`, `path`, `component`, `title`, `icon`, `sort_no`, `hidden`, `always_show`, `menu_id`) VALUES ('8ab9d1e4197343dcaf43f77d9eab04fa', 'Config', '/config', 'serverconfig/ServerConfig', '服务配置', 'dashboard', '4', '\0', '\0', 'a122ecc013714c5ca32354fc24112fb5');


-- 角色表
DELETE FROM `role`;
INSERT INTO `role` (`role_id`, `name`, `remark`) VALUES ('2c033d577bea44aa825ffae125c7367e', '管理员', '所有功能');
INSERT INTO `role` (`role_id`, `name`, `remark`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', '用户组', '首页、内网穿透的功能');

-- 权限表
DELETE FROM `authority`;
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', '1c4fd2a0a80b4d44acb54fd7887b7297');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', '5aaa053dcbe1409a99801a5803c060a8');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', '471efa4ffa5145bf8f910395a061b71c');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', '7966d831e5b9497f9c9da8823117579a');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('567ad53a9b2f41b4a6e65fb7e88449b2', 'a8e634b4ed914267970352e287057610');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '1c4fd2a0a80b4d44acb54fd7887b7297');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '5aaa053dcbe1409a99801a5803c060a8');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '090756bdc3e64549a8c38de968f0bec4');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '4d46153bc5794d36a08b69fc8f979f77');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '47292ce0101a430f9a3f72c566306a8f');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '471efa4ffa5145bf8f910395a061b71c');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '7966d831e5b9497f9c9da8823117579a');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', 'a8e634b4ed914267970352e287057610');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', 'a122ecc013714c5ca32354fc24112fb5');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '8ab9d1e4197343dcaf43f77d9eab04fa');
INSERT INTO `supernat`.`authority` (`role_id`, `menu_id`) VALUES ('2c033d577bea44aa825ffae125c7367e', '0');