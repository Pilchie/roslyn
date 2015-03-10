// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
{
    internal partial class Controller
    {
        internal partial class Session : Session<Controller, Model, ISignatureHelpPresenterSession>
        {
            public Session(Controller controller, ISignatureHelpPresenterSession presenterSession)
                : base(controller, new ModelComputation<Model>(controller, TaskScheduler.Default), presenterSession)
            {
                this.PresenterSession.ItemSelected += OnPresenterSessionItemSelected;
            }

            public override void Stop()
            {
                AssertIsForeground();
                this.PresenterSession.ItemSelected -= OnPresenterSessionItemSelected;
                base.Stop();
            }

            private void OnPresenterSessionItemSelected(object sender, SignatureHelpItemEventArgs e)
            {
                AssertIsForeground();
                Contract.ThrowIfFalse(ReferenceEquals(this.PresenterSession, sender));

                SetModelExplicitlySelectedItem(m => e.SignatureHelpItem);
            }
        }
    }
}
