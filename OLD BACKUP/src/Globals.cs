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
global using MoreSlugcats;
global using BepInEx;
global using BepInEx.Logging;
global using Fisobs.Core;
global using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;
global using static Slugpack.Constants;
global using static Slugpack.Logs;
global using static Slugpack.Utilities;
global using static Pom.Pom;
global using static Pom.Pom.Vector2ArrayField;
global using Fisobs.Creatures;
global using Fisobs.Sandbox;
global using LizardCosmetics;
global using RWCustom;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]