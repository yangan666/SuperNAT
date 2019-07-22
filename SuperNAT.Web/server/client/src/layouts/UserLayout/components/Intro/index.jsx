import React from 'react';
import styles from './index.module.scss';

const LoginIntro = () => {
  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <div className={styles.title}>SuperNat管理后台</div>
        <p className={styles.description}>触达更多优质用户，轻松创建内网穿透</p>
        <p className={styles.description}>抢占优质首发技能，抢注优质技能调用名称</p>
        <p className={styles.description}>一起解构数字世界，碰撞科技创新思想</p>
      </div>
    </div>
  );
};
export default LoginIntro;
