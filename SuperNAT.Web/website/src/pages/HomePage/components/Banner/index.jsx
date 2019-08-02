import React from 'react';
import styles from './index.module.scss';

export default function Banner() {
  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <div className={styles.title}>SuperNAT 内网穿透</div>
        <div className={styles.desc}>SuperNAT & 让内网穿透简单而友好</div>
        <a className={styles.link}>开始使用</a>
      </div>
    </div>
  );
}
