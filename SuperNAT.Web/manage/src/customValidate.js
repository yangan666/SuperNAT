import { Validator } from "vee-validate"

// Validator.extend("password", {
//   messages: {
//     zh_CN: field => field + "不能少于十位数"
//   },
//   validate: value => {
//     return value
//   }
// })
Validator.extend("confirmPassword", {
  messages: {
    zh_CN: field => "两次输入密码不一致"
  },
  validate: (val1, val2) => {
    return val1 === val2
  }
})
