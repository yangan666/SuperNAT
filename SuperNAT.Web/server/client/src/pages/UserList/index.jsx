import React, { useState } from 'react';
import TopBar from '@/components/TopBar';
import GeneralDialog from '@/components/GeneralDialog';
import UsersTable from './components/UsersTable';
import mockdata from './data';

export default function UserList() {
  const [tableData, setTableData] = useState(mockdata);

  const getFormValue = (value) => {
    const data = [...tableData];
    data.push({
      id: data.length + 1,
      name: value.title,
      desc: value.desc,
      preview: '--',
      skill: '无',
    });
    setTableData(data);
  };
  return (
    <div>
      <TopBar
        title="用户管理"
        extraAfter={<GeneralDialog buttonText="新建用户" getFormValue={getFormValue} />}
      />
      <UsersTable data={tableData} />
    </div>
  );
}
