﻿using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Graph step types
/// </summary>
[PublicAPI]
public enum GraphStepTypes {
    [Description("None")] None,
    [Description("Free reverse rotation")] FreeReverseRotation,
    [Description("Thread tap")] ThreadTap,
    [Description("Engaging")] Engaging,
    [Description("Free rotation")] FreeRotation,
    [Description("Fastening")] Fastening,
    [Description("Snug torque")] SnugTorque,
    [Description("Prevailing start")] Prevailing,
    [Description("Seating")] Seating,
    [Description("Clamp")] Clamp,
    [Description("Torque complete")] TorqueComplete,

    [Description("Rotation after torque-up")] RotationAfterTorqueUp
}