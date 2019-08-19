import React from 'react';
import { Input, Table } from '@alifd/next';
import styles from './index.module.scss';

export default function UsersTable({ data, operate }) {
  const renderOper = (value, index, record) => {
    return (
      <div className={styles.oper}>
        <a className={styles.button} onClick={() => { operate('edit', record) }}>
          编辑
        </a>
        <a className={styles.button} onClick={() => { operate('delete', record) }}>
          删除
        </a>
      </div>
    );
  };
  return (
    <div>
      <div className={styles.searchBar}>
        <div className={styles.info}>共 {data.dataSource.length} 条记录</div>
        <Input
          style={{ width: '300px' }}
          placeholder="请输入关键字"
        />
      </div>
      <Table loading={data.__loading} hasBorder={false} dataSource={data.dataSource}>
        <Table.Column width={150} title="所属用户" dataIndex="user_name" />
        <Table.Column width={200} title="应用名称" dataIndex="name" />
        <Table.Column width={200} title="内网地址" dataIndex="local" />
        <Table.Column width={200} title="外网地址" dataIndex="remote" />
        <Table.Column width={150} title="协议类型" dataIndex="protocol" />
        <Table.Column width={350} title="证书文件" dataIndex="certfile" />
        <Table.Column title="操作" cell={renderOper} />
      </Table>
    </div>
  );
}
