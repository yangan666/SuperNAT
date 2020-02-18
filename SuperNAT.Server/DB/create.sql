/*
Navicat MySQL Data Transfer

Source Server         : 139.155.104.69
Source Server Version : 50505
Source Host           : 139.155.104.69:3306
Source Database       : supernat

Target Server Type    : MYSQL
Target Server Version : 50505
File Encoding         : 65001

Date: 2020-02-18 14:23:28
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for authority
-- ----------------------------
DROP TABLE IF EXISTS `authority`;
CREATE TABLE `authority` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `role_id` varchar(50) DEFAULT '',
  `menu_id` varchar(50) DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=42 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for client
-- ----------------------------
DROP TABLE IF EXISTS `client`;
CREATE TABLE `client` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL DEFAULT '',
  `subdomain` varchar(150) DEFAULT '',
  `secret` varchar(50) NOT NULL DEFAULT '',
  `is_online` bit(1) NOT NULL DEFAULT b'0',
  `last_heart_time` datetime DEFAULT NULL,
  `machine_id` varchar(150) DEFAULT '',
  `remark` varchar(500) DEFAULT NULL,
  `user_id` varchar(50) DEFAULT '',
  `create_time` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for map
-- ----------------------------
DROP TABLE IF EXISTS `map`;
CREATE TABLE `map` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(250) NOT NULL DEFAULT '',
  `local` varchar(150) NOT NULL DEFAULT '',
  `local_port` int(11) DEFAULT NULL,
  `remote` varchar(150) NOT NULL DEFAULT '',
  `remote_port` int(11) DEFAULT NULL,
  `protocol` varchar(50) NOT NULL DEFAULT '',
  `certfile` varchar(150) DEFAULT '',
  `certpwd` varchar(150) DEFAULT NULL,
  `is_ssl` bit(1) NOT NULL DEFAULT b'0',
  `ssl_type` int(11) DEFAULT NULL,
  `client_id` int(11) NOT NULL,
  `is_disabled` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for menu
-- ----------------------------
DROP TABLE IF EXISTS `menu`;
CREATE TABLE `menu` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `pid` varchar(50) DEFAULT '',
  `name` varchar(100) DEFAULT '',
  `path` varchar(150) DEFAULT '',
  `component` varchar(150) DEFAULT '',
  `title` varchar(150) NOT NULL DEFAULT '',
  `icon` varchar(50) DEFAULT '',
  `sort_no` int(11) NOT NULL,
  `hidden` bit(1) NOT NULL DEFAULT b'0',
  `always_show` bit(1) NOT NULL DEFAULT b'0',
  `menu_id` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for request
-- ----------------------------
DROP TABLE IF EXISTS `request`;
CREATE TABLE `request` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `request_url` varchar(500) DEFAULT NULL,
  `client_ip` varchar(100) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `request_time` datetime DEFAULT NULL,
  `response_time` datetime DEFAULT NULL,
  `handle_time` datetime DEFAULT NULL,
  `create_time` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for role
-- ----------------------------
DROP TABLE IF EXISTS `role`;
CREATE TABLE `role` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `role_id` varchar(36) NOT NULL,
  `name` varchar(150) NOT NULL DEFAULT '',
  `remark` varchar(250) DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for server_config
-- ----------------------------
DROP TABLE IF EXISTS `server_config`;
CREATE TABLE `server_config` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `protocol` varchar(50) NOT NULL,
  `port` varchar(255) NOT NULL DEFAULT '',
  `is_ssl` bit(1) NOT NULL,
  `ssl_type` int(11) DEFAULT NULL,
  `certfile` varchar(150) DEFAULT NULL,
  `certpwd` varchar(150) DEFAULT NULL,
  `is_disabled` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` varchar(50) DEFAULT '',
  `user_name` varchar(150) NOT NULL DEFAULT '',
  `password` varchar(150) NOT NULL DEFAULT '',
  `wechat` varchar(150) DEFAULT '',
  `email` varchar(150) DEFAULT NULL,
  `tel` varchar(50) DEFAULT '',
  `is_disabled` bit(1) NOT NULL DEFAULT b'0',
  `is_admin` bit(1) NOT NULL DEFAULT b'0',
  `role_id` varchar(50) DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=39 DEFAULT CHARSET=utf8;
