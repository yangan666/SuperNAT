import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import UsersTable from './components/UsersTable';
import UserDialog from './components/UserDialog';
import CustomDataBinder from '@/utils/databinder'
import { Dialog, Button } from '@alifd/next';

@CustomDataBinder({
  userList: {
    url: '/Api/User/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    },
    showSuccessToast: false
  },
  userData: {
    url: '/Api/User/GetOne',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    },
    showSuccessToast: false
  },
  addUser: {
    url: '/Api/User/Add',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  delUser: {
    url: '/Api/User/Delete',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  disable: {
    url: '/Api/User/Disable',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  }
})
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
          this.setState({ user: res.data.dataSource })
        });
      }
      this.setState({ dialogVisible: value })
    }
    const operate = (type, record) => {
      switch (type) {
        case 'edit':
          this.props.updateBindingData('userData', {
            data: { id: record.id }
          }, (res) => {
            this.setState({ user: res.data.dataSource || {}, dialogVisible: true })
          });
          break;
        case 'disable':
          Dialog.confirm({
            title: '提示',
            content: `确定${record.is_disabled ? '启用' : '禁用'}用户"${record.user_name}"吗`,
            onOk: () => {
              this.props.updateBindingData('disable', {
                data: record
              }, (res) => {
                if (res.status == "SUCCESS") {
                  this.props.updateBindingData('userList', {
                    data: {}
                  });
                }
              });
            }
          });
          break;
        case 'delete':
          Dialog.confirm({
            title: '提示',
            content: `确定删除用户"${record.user_name}"吗`,
            onOk: () => {
              this.props.updateBindingData('delUser', {
                data: { id: record.id }
              }, (res) => {
                if (res.status == "SUCCESS") {
                  this.props.updateBindingData('userList', {
                    data: {}
                  });
                }
              });
            }
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
        <UsersTable data={userList} operate={operate} />
        <UserDialog dialogTitle="新建用户"
          dialogVisible={this.state.dialogVisible}
          getFormValue={getFormValue}
          setVisible={setVisible}
          formData={this.state.user} />
      </div>
    );
  }
}
