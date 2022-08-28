﻿using MasterServerToolkit.MasterServer;
using MasterServerToolkit.UI;
using UnityEngine;

namespace MasterServerToolkit.Games
{
    public class OkDialogBoxView : PopupViewComponent
    {
        protected override void Awake()
        {
            base.Awake();

            Mst.Events.AddListener(MstEventKeys.showOkDialogBox, OnShowDialogBoxEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideOkDialogBox, OnHideDialogBoxEventHandler);
        }

        private void OnShowDialogBoxEventHandler(EventMessage message)
        {
            var messageData = message.As<OkDialogBoxEventMessage>();

            SetLables(messageData.Message);

            SetButtonsClick(() =>
            {
                messageData.OkCallback?.Invoke();
                Hide();
            });

            Show();
        }

        private void OnHideDialogBoxEventHandler(EventMessage message)
        {
            Hide();
        }
    }
}
