using NUnit.Framework;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace FifthSemester.Tests
{
    public class PlayerComponentsTests
    {
        private Type FindType(string shortName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.Name == shortName);
        }

        [Test]
        public void PlayerController_Awake_WiresComponents()
        {
            GameObject go = new GameObject("Player");

            Type controllerType = FindType("PlayerController");
            Assert.IsNotNull(controllerType, "PlayerController type not found.");

            Component controllerComp = go.AddComponent(controllerType);

            // Add required components by name via reflection
            Type movementType = FindType("PlayerMovement");
            Type jumpType = FindType("PlayerJump");
            Type cameraType = FindType("PlayerCamera");
            Type interactionType = FindType("PlayerInteraction");

            Assert.IsNotNull(movementType, "PlayerMovement type not found.");
            Assert.IsNotNull(jumpType, "PlayerJump type not found.");
            Assert.IsNotNull(cameraType, "PlayerCamera type not found.");
            Assert.IsNotNull(interactionType, "PlayerInteraction type not found.");

            go.AddComponent(movementType);
            go.AddComponent(jumpType);
            go.AddComponent(cameraType);
            go.AddComponent(interactionType);
            go.AddComponent<Rigidbody>();

            // Invoke Awake on controller
            MethodInfo awake = controllerType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            if (awake != null) awake.Invoke(controllerComp, null);

            // Assert properties are assigned (use properties if present)
            PropertyInfo prop;

            prop = controllerType.GetProperty("PlayerMovement", BindingFlags.Public | BindingFlags.Instance);
            var playerMovementVal = prop != null ? prop.GetValue(controllerComp) : null;
            Assert.IsNotNull(playerMovementVal);

            prop = controllerType.GetProperty("PlayerJump", BindingFlags.Public | BindingFlags.Instance);
            var playerJumpVal = prop != null ? prop.GetValue(controllerComp) : null;
            Assert.IsNotNull(playerJumpVal);

            prop = controllerType.GetProperty("PlayerCamera", BindingFlags.Public | BindingFlags.Instance);
            var playerCameraVal = prop != null ? prop.GetValue(controllerComp) : null;
            Assert.IsNotNull(playerCameraVal);

            prop = controllerType.GetProperty("PlayerInteraction", BindingFlags.Public | BindingFlags.Instance);
            var playerInteractionVal = prop != null ? prop.GetValue(controllerComp) : null;
            Assert.IsNotNull(playerInteractionVal);

            prop = controllerType.GetProperty("Rigidbody", BindingFlags.Public | BindingFlags.Instance);
            var rbVal = prop != null ? prop.GetValue(controllerComp) : null;
            Assert.IsNotNull(rbVal);

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
