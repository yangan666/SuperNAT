import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import UsersTable from './components/UsersTable';
import UserDialog from './components/UserDialog';
import DataBinder from '@icedesign/data-binder';
import request from '@/utils/request'
import { Dialog, Input, Message, Button } from '@alifd/next';

@DataBinder({
  userList: {
    url: '/Api/User/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    }
  },
  userData: {
    url: '/Api/User/GetOne',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  addUser: {
    url: '/Api/User/Add',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  }
}, { requestClient: request })
export default class UserList extends Component {
  constructor(props) {
    super(props);
    this.state = { dialogVisible: false, user: {} };
  }
  componentDidMount() {
    // 组件加载时获取数据源，数据获取完成会触发组件 render
    this.props.updateBindingData('userList', {
      data: {}
    });
  }
  render() {
    const { userList } = this.props.bindingData
    const tableData = userList.dataSource
    const getFormValue = (value) => {
      //保存
      this.props.updateBindingData('addUser', {
        data: value
      }, (res) => {
        if (res.status == "SUCCESS") {
          this.props.updateBindingData('userList', {
            data: {}
          });
        }
      });
    };
    const setVisible = (value) => {
      if (value) {
        //新建用户
        this.props.updateBindingData('userData', {
          data: {}
        }, (res) => {
          this.setState({ user: res.data.dataSource || {} })
        });
      }
      this.setState({ dialogVisible: value })
    }
    const operate = (type, id) => {
      console.log(type, id)
      switch (type) {
        case 'edit':
          this.props.updateBindingData('userData', {
            data: { id }
          }, (res) => {
            this.setState({ user: res.data.dataSource || {}, dialogVisible: true })
            console.log(this.state.user);
          });
          break;
      }
    };
    return (
      <div>
        <TopBar
          title="用户管理"
          extraAfter={<Button type="primary" onClick={() => { setVisible(true) }}>新建用户</Button>}
        />
        <UsersTable data={tableData} operate={operate} />
        <UserDialog dialogTitle="新建用户"
          dialogVisible={this.state.dialogVisible}
          getFormValue={getFormValue}
          setVisible={setVisible}
          formData={this.state.user} />
      </div>
    );
  }
}
