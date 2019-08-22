import React from 'react';
import { Input, Table } from '@alifd/next';
import IceLabel from '@icedesign/label';
import styles from './index.module.scss';

export default function ClientTable({ data, operate }) {
  const renderStatus = (value, index, record) => {
    return (
      <IceLabel status={record.is_online ? 'success' : 'danger'}>{record.is_online_str}</IceLabel>
    );
  }
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
  const tableData = data.dataSource || []
  return (
    <div>
      <div className={styles.searchBar}>
        <div className={styles.info}>共 {tableData.length} 条记录</div>
        <Input
          style={{ width: '300px' }}
          placeholder="请输入关键字"
        />
      </div>
      <Table loading={data.__loading} hasBorder={false} dataSource={tableData}>
        <Table.Column width={150} title="所属用户" dataIndex="user_name" />
        <Table.Column width={150} title="主机名称" dataIndex="name" />
        <Table.Column width={280} title="主机密钥" dataIndex="secret" />
        <Table.Column width={150} title="二级域名" dataIndex="subdomain" />
        <Table.Column width={180} title="最后活动时间" dataIndex="last_heart_time" />
        <Table.Column width={180} title="创建时间" dataIndex="create_time" />
        <Table.Column width={300} title="描述" dataIndex="remark" />
        <Table.Column width={100} title="状态" cell={renderStatus} />
        <Table.Column title="操作" cell={renderOper} />
      </Table>
    </div>
  );
}
