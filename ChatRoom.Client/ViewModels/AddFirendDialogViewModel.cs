using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using Prism.Dialogs;
using Prism.Commands;
using Prism.Mvvm;

namespace ChatRoom.Client.ViewModels
{
    public class AddFirendDialogViewModel : BindableBase, IDialogAware
    {
        // 好友名称属性
        private string _friendName = string.Empty;

        public string FriendName
        {
            get => _friendName;
            set => SetProperty(ref _friendName, value);
        }

        // 确认命令
        public DelegateCommand ConfirmCommand { get; set; }
        // 取消命令
        public DelegateCommand CancelCommand { get; set; }

        public AddFirendDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
            CancelCommand = new DelegateCommand(Cancel);
        }

        // 对话框关闭事件
        public DialogCloseListener RequestClose { get; set; }

        /// <summary>
        /// 确认添加好友
        /// </summary>
        private void Confirm()
        {
            var parameters = new DialogParameters
            {
                { "friendName", FriendName }
            };

            // 关闭对话框并返回结果
            RequestClose.Invoke(parameters, ButtonResult.OK);
        }
        
        private void Cancel()
        {
            // 关闭对话框并返回结果
            RequestClose.Invoke(new DialogParameters(), ButtonResult.Cancel);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }
    }
}

