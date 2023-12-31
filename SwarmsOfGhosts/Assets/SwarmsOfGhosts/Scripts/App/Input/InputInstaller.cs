﻿using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Input
{
    [CreateAssetMenu(fileName = "InputInstaller", menuName = "Installers/InputInstaller")]
    public class InputInstaller : ScriptableObjectInstaller<InputInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<InputViewModel>()
                .AsSingle();

            Container
                .BindInterfacesTo<InputView>()
                .AsSingle();
        }
    }
}