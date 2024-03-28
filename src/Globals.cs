global using DevInterface;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Security;
global using System.Security.Permissions;
global using UnityEngine;
global using static Slugpack.DataStructures;
global using static Slugpack.WeakTables;
global using CreatureType = CreatureTemplate.Type;
global using Color = UnityEngine.Color;
global using Random = UnityEngine.Random;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]