// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using XRTK.Definitions;
using XRTK.Definitions.Platforms;
using XRTK.Extensions;
using XRTK.Interfaces;
using XRTK.Interfaces.BoundarySystem;
using XRTK.Interfaces.CameraSystem;
using XRTK.Interfaces.DiagnosticsSystem;
using XRTK.Interfaces.InputSystem;
using XRTK.Interfaces.NetworkingSystem;
using XRTK.Interfaces.SpatialAwarenessSystem;
using XRTK.Interfaces.TeleportSystem;
using XRTK.Utilities;
using XRTK.Utilities.Async;

namespace XRTK.Services
{
    /// <summary>
    /// This class is responsible for coordinating the operation of the Mixed Reality Toolkit. It is the only Singleton in the entire project.
    /// It provides a service registry for all active services that are used within a project as well as providing the active profile for the project.
    /// The <see cref="ActiveProfile"/> can be swapped out at any time to meet the needs of your project.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public sealed class MixedRealityToolkit : MonoBehaviour, IDisposable
    {
        #region Mixed Reality Toolkit Profile properties

        /// <summary>
        /// Checks if there is a valid instance of the MixedRealityToolkit, then checks if there is there a valid Active Profile.
        /// </summary>
        public static bool HasActiveProfile
        {
            get
            {
                if (!IsInitialized)
                {
                    return false;
                }

                if (!ConfirmInitialized())
                {
                    return false;
                }

                return Instance.ActiveProfile != null;
            }
        }

        /// <summary>
        /// The active profile of the Mixed Reality Toolkit which controls which services are active and their initial settings.
        /// *Note a profile is used on project initialization or replacement, changes to properties while it is running has no effect.
        /// </summary>
        [SerializeField]
        [Tooltip("The current active settings for the Mixed Reality project")]
        private MixedRealityToolkitRootProfile activeProfile = null;

        /// <summary>
        /// The public property of the Active Profile, ensuring events are raised on the change of the reference
        /// </summary>
        public MixedRealityToolkitRootProfile ActiveProfile
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying &&
                    activeProfile.IsNull() &&
                    UnityEditor.Selection.activeObject != Instance)
                {
                    UnityEditor.Selection.activeObject = Instance;
                }
#endif // UNITY_EDITOR
                return activeProfile;
            }
            set
            {
                ResetProfile(value);
            }
        }

        /// <summary>
        /// When a profile is replaced with a new one, force all services to reset and read the new values
        /// </summary>
        /// <param name="profile"></param>
        public void ResetProfile(MixedRealityToolkitRootProfile profile)
        {
            if (isResetting)
            {
                Debug.LogWarning("Already attempting to reset the root profile!");
                return;
            }

            if (isInitializing)
            {
                Debug.LogWarning("Already attempting to initialize the root profile!");
                return;
            }

            isResetting = true;

            if (!activeProfile.IsNull())
            {
                DisableAllServices();
                DestroyAllServices();
            }

            activeProfile = profile;

            if (!profile.IsNull())
            {
                DisableAllServices();
                DestroyAllServices();
            }

            EnsureMixedRealityRequirements();
            InitializeServiceLocator();

            isResetting = false;
        }

        private static bool isResetting = false;

        #endregion Mixed Reality Toolkit Profile properties

        #region Mixed Reality runtime service registry

        // ReSharper disable once InconsistentNaming
        private static readonly List<IMixedRealityPlatform> availablePlatforms = new List<IMixedRealityPlatform>();

        /// <summary>
        /// The list of active platforms detected by the <see cref="MixedRealityToolkit"/>.
        /// </summary>
        public static IReadOnlyList<IMixedRealityPlatform> AvailablePlatforms => availablePlatforms;

        // ReSharper disable once InconsistentNaming
        private static readonly List<IMixedRealityPlatform> activePlatforms = new List<IMixedRealityPlatform>();

        /// <summary>
        /// The list of active platforms detected by the <see cref="MixedRealityToolkit"/>.
        /// </summary>
        public static IReadOnlyList<IMixedRealityPlatform> ActivePlatforms => activePlatforms;

        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<Type, IMixedRealityService> activeSystems = new Dictionary<Type, IMixedRealityService>();

        /// <summary>
        /// Current active systems registered with the MixedRealityToolkit.
        /// </summary>
        /// <remarks>
        /// Systems can only be registered once by <see cref="Type"/> and are executed in a specific priority order.
        /// </remarks>
        public static IReadOnlyDictionary<Type, IMixedRealityService> ActiveSystems => activeSystems;

        // ReSharper disable once InconsistentNaming
        private static readonly List<Tuple<Type, IMixedRealityService>> registeredMixedRealityServices = new List<Tuple<Type, IMixedRealityService>>();

        /// <summary>
        /// Local service registry for the Mixed Reality Toolkit, to allow runtime use of the <see cref="IMixedRealityService"/>.
        /// </summary>
        /// <remarks>
        /// Services can have one or more instances registered and can be executed simultaneously. Best to get them out by name or guid.
        /// </remarks>
        public static IReadOnlyList<Tuple<Type, IMixedRealityService>> RegisteredMixedRealityServices => registeredMixedRealityServices;

        #endregion Mixed Reality runtime service registry

        #region Instance Management

        private static bool isGettingInstance = false;

        /// <summary>
        /// Returns the Singleton instance of the classes type.
        /// If no instance is found, then we search for an instance in the scene.
        /// If more than one instance is found, we log an error and no instance is returned.
        /// </summary>
        public static MixedRealityToolkit Instance
        {
            get
            {
                if (IsInitialized)
                {
                    return instance;
                }

                if (isGettingInstance ||
                   (Application.isPlaying && !searchForInstance))
                {
                    return null;
                }

                isGettingInstance = true;

                var objects = FindObjectsOfType<MixedRealityToolkit>();
                searchForInstance = false;
                MixedRealityToolkit newInstance;

                switch (objects.Length)
                {
                    case 0:
                        newInstance = new GameObject(nameof(MixedRealityToolkit)).AddComponent<MixedRealityToolkit>();
                        break;
                    case 1:
                        newInstance = objects[0];
                        break;
                    default:
                        Debug.LogError($"Expected exactly 1 {nameof(MixedRealityToolkit)} but found {objects.Length}.");
                        isGettingInstance = false;
                        return null;
                }

                if (newInstance == null)
                {
                    Debug.LogError("Failed to get instance!");
                    isGettingInstance = false;
                    return null;
                }

                if (!IsApplicationQuitting)
                {
                    // Setup any additional things the instance needs.
                    newInstance.InitializeInstance();
                }
                else
                {
                    // Don't do any additional setup because the app is quitting.
                    instance = newInstance;
                }

                if (instance == null)
                {
                    Debug.LogError("Failed to get instance!");
                    isGettingInstance = false;
                    return null;
                }

                isGettingInstance = false;
                return instance;
            }
        }

        private static MixedRealityToolkit instance;

        /// <summary>
        /// Lock property for the Mixed Reality Toolkit to prevent reinitialization
        /// </summary>
        private static readonly object InitializedLock = new object();

        private void InitializeInstance()
        {
            lock (InitializedLock)
            {
                if (IsInitialized) { return; }

                instance = this;

                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(instance.transform.root);
                }

                Application.quitting += () => IsApplicationQuitting = true;

#if UNITY_EDITOR
                UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
                UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                void OnHierarchyChanged()
                {
                    if (instance != null)
                    {
                        Debug.Assert(instance.transform.parent == null, "The MixedRealityToolkit should not be parented under any other GameObject!");
                        Debug.Assert(instance.transform.childCount == 0, "The MixedRealityToolkit should not have GameObject children!");
                    }
                }

                void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
                {
                    if (playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode ||
                        playModeState == UnityEditor.PlayModeStateChange.EnteredEditMode)
                    {
                        IsApplicationQuitting = false;
                    }

                    if (activeProfile == null &&
                        playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode)
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                        UnityEditor.Selection.activeObject = Instance;
                        UnityEditor.EditorApplication.delayCall += () =>
                            UnityEditor.EditorGUIUtility.PingObject(Instance);
                    }
                }
#endif // UNITY_EDITOR

                EnsureMixedRealityRequirements();

                if (HasActiveProfile)
                {
                    InitializeServiceLocator();
                }
            }
        }

        /// <summary>
        /// Flag to search for instance the first time Instance property is called.
        /// Subsequent attempts will generally switch this flag false, unless the instance was destroyed.
        /// </summary>
        private static bool searchForInstance = true;

        private static bool isInitializing = false;

        /// <summary>
        /// Flag stating if the application is currently attempting to quit.
        /// </summary>
        public static bool IsApplicationQuitting { get; private set; } = false;

        /// <summary>
        /// Expose an assertion whether the MixedRealityToolkit class is initialized.
        /// </summary>
        public static void AssertIsInitialized()
        {
            Debug.Assert(IsInitialized, "The MixedRealityToolkit has not been initialized.");
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized => instance != null;

        /// <summary>
        /// Static function to determine if the <see cref="MixedRealityToolkit"/> class has been initialized or not.
        /// </summary>
        public static bool ConfirmInitialized()
        {
            var access = Instance;
            Debug.Assert(IsInitialized.Equals(access != null));
            return IsInitialized;
        }

        /// <summary>
        /// Once all services are registered and properties updated, the Mixed Reality Toolkit will initialize all active services.
        /// This ensures all services can reference each other once started.
        /// </summary>
        private void InitializeServiceLocator()
        {
            if (isInitializing)
            {
                Debug.LogWarning("Already attempting to initialize the service locator!");
                return;
            }

            isInitializing = true;

            //If the Mixed Reality Toolkit is not configured, stop.
            if (ActiveProfile == null)
            {
                Debug.LogError("No Mixed Reality Root Profile found, cannot initialize the Mixed Reality Toolkit");
                isInitializing = false;
                return;
            }

#if UNITY_EDITOR
            if (ActiveSystems.Count > 0)
            {
                activeSystems.Clear();
            }

            if (RegisteredMixedRealityServices.Count > 0)
            {
                registeredMixedRealityServices.Clear();
            }
#endif

            Debug.Assert(ActiveSystems.Count == 0);
            Debug.Assert(RegisteredMixedRealityServices.Count == 0);
            Debug.Assert(ActivePlatforms.Count > 0, "No Active Platforms found!");

            ClearCoreSystemCache();

            #region Services Registration

            if (ActiveProfile.IsCameraSystemEnabled)
            {
                if (TryCreateAndRegisterService<IMixedRealityCameraSystem>(ActiveProfile.CameraSystemType, out var service, ActiveProfile.CameraSystemProfile) && CameraSystem != null)
                {
                    TryRegisterDataProviderConfigurations(ActiveProfile.CameraSystemProfile.RegisteredServiceConfigurations, service);
                }
                else
                {
                    Debug.LogError("Failed to start the Camera System!");
                }
            }

            if (ActiveProfile.IsInputSystemEnabled)
            {
#if UNITY_EDITOR
                // Make sure unity axis mappings are set.
                Utilities.Editor.InputMappingAxisUtility.CheckUnityInputManagerMappings(Definitions.Devices.ControllerMappingLibrary.UnityInputManagerAxes);
#endif

                if (TryCreateAndRegisterService<IMixedRealityInputSystem>(ActiveProfile.InputSystemType, out var service, ActiveProfile.InputSystemProfile) && InputSystem != null)
                {
                    if (TryCreateAndRegisterService<IMixedRealityFocusProvider>(ActiveProfile.InputSystemProfile.FocusProviderType, out _))
                    {
                        TryRegisterDataProviderConfigurations(ActiveProfile.InputSystemProfile.RegisteredServiceConfigurations, service);
                    }
                    else
                    {
                        inputSystem = null;
                        Debug.LogError("Failed to register the focus provider! The input system will not function without it.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to start the Input System!");
                }
            }
            else
            {
#if UNITY_EDITOR
                Utilities.Editor.InputMappingAxisUtility.RemoveMappings(Definitions.Devices.ControllerMappingLibrary.UnityInputManagerAxes);
#endif
            }

            if (ActiveProfile.IsBoundarySystemEnabled)
            {
                if (!TryCreateAndRegisterService<IMixedRealityBoundarySystem>(ActiveProfile.BoundarySystemSystemType, out _, ActiveProfile.BoundaryVisualizationProfile) || BoundarySystem == null)
                {
                    Debug.LogError("Failed to start the Boundary System!");
                }
            }

            if (ActiveProfile.IsSpatialAwarenessSystemEnabled)
            {
#if UNITY_EDITOR
                // Setup the default spatial awareness layers in the project settings.
                LayerExtensions.SetupLayer(31, Definitions.SpatialAwarenessSystem.MixedRealitySpatialAwarenessSystemProfile.SpatialAwarenessMeshesLayerName);
                LayerExtensions.SetupLayer(30, Definitions.SpatialAwarenessSystem.MixedRealitySpatialAwarenessSystemProfile.SpatialAwarenessSurfacesLayerName);
#endif
                if (TryCreateAndRegisterService<IMixedRealitySpatialAwarenessSystem>(ActiveProfile.SpatialAwarenessSystemSystemType, out var service, ActiveProfile.SpatialAwarenessProfile) && SpatialAwarenessSystem != null)
                {
                    TryRegisterDataProviderConfigurations(ActiveProfile.SpatialAwarenessProfile.RegisteredServiceConfigurations, service);
                }
                else
                {
                    Debug.LogError("Failed to start the Spatial Awareness System!");
                }
            }
            else
            {
#if UNITY_EDITOR
                LayerExtensions.RemoveLayer(Definitions.SpatialAwarenessSystem.MixedRealitySpatialAwarenessSystemProfile.SpatialAwarenessMeshesLayerName);
                LayerExtensions.RemoveLayer(Definitions.SpatialAwarenessSystem.MixedRealitySpatialAwarenessSystemProfile.SpatialAwarenessSurfacesLayerName);
#endif
            }

            if (ActiveProfile.IsTeleportSystemEnabled)
            {
                // Note: The Teleport system doesn't have a profile, but might in the future.
                var dummyProfile = ScriptableObject.CreateInstance<MixedRealityToolkitRootProfile>();

                if (!TryCreateAndRegisterService<IMixedRealityTeleportSystem>(ActiveProfile.TeleportSystemSystemType, out _, dummyProfile) || TeleportSystem == null)
                {
                    Debug.LogError("Failed to start the Teleport System!");
                }
            }

            if (ActiveProfile.IsNetworkingSystemEnabled)
            {
                if (TryCreateAndRegisterService<IMixedRealityNetworkingSystem>(ActiveProfile.NetworkingSystemSystemType, out var service, ActiveProfile.NetworkingSystemProfile) && NetworkingSystem != null)
                {
                    TryRegisterDataProviderConfigurations(ActiveProfile.NetworkingSystemProfile.RegisteredServiceConfigurations, service);
                }
                else
                {
                    Debug.LogError("Failed to start the Networking System!");
                }
            }

            if (ActiveProfile.IsDiagnosticsSystemEnabled)
            {
                if (TryCreateAndRegisterService<IMixedRealityDiagnosticsSystem>(ActiveProfile.DiagnosticsSystemSystemType, out var service, ActiveProfile.DiagnosticsSystemProfile) && DiagnosticsSystem != null)
                {
                    TryRegisterDataProviderConfigurations(ActiveProfile.DiagnosticsSystemProfile.RegisteredServiceConfigurations, service);
                }
                else
                {
                    Debug.LogError("Failed to start the Diagnostics System!");
                }
            }

            if (ActiveProfile.RegisteredServiceProvidersProfile != null &&
                ActiveProfile.RegisteredServiceProvidersProfile.RegisteredServiceConfigurations != null)
            {
                TryRegisterServiceConfigurations(ActiveProfile.RegisteredServiceProvidersProfile.RegisteredServiceConfigurations);
            }

            #endregion Service Registration

            #region Services Initialization

            var orderedCoreSystems = activeSystems.OrderBy(m => m.Value.Priority).ToArray();
            activeSystems.Clear();

            foreach (var system in orderedCoreSystems)
            {
                TryRegisterServiceInternal(system.Key, system.Value);
            }

            var orderedServices = registeredMixedRealityServices.OrderBy(service => service.Item2.Priority).ToArray();
            registeredMixedRealityServices.Clear();

            foreach (var (interfaceType, mixedRealityService) in orderedServices)
            {
                TryRegisterServiceInternal(interfaceType, mixedRealityService);
            }

            InitializeAllServices();

            #endregion Services Initialization

            isInitializing = false;
        }

        private static void EnsureMixedRealityRequirements()
        {
            activePlatforms.Clear();
            availablePlatforms.Clear();

            var platformTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMixedRealityPlatform).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .OrderBy(type => type.Name);

            var platformOverrides = new List<Type>();

            foreach (var platformType in platformTypes)
            {
                IMixedRealityPlatform platform = null;

                try
                {
                    platform = Activator.CreateInstance(platformType) as IMixedRealityPlatform;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (platform == null) { continue; }

                availablePlatforms.Add(platform);

                if (platform.IsAvailable ||
                    platform.IsBuildTargetAvailable)
                {
                    foreach (var platformOverride in platform.PlatformOverrides)
                    {
                        platformOverrides.Add(platformOverride.GetType());
                    }
                }
            }

            foreach (var platform in availablePlatforms)
            {
                if (platformOverrides.Contains(platform.GetType()))
                {
                    continue;
                }

                if (platform.IsAvailable ||
                    platform.IsBuildTargetAvailable)
                {
                    activePlatforms.Add(platform);
                }
            }

            // There's lots of documented cases that if the camera doesn't start at 0,0,0, things break with the WMR SDK specifically.
            // We'll enforce that here, then tracking can update it to the appropriate position later.
            CameraCache.Main.transform.position = Vector3.zero;

            bool addedComponents = false;

            if (!Application.isPlaying)
            {
                var eventSystems = FindObjectsOfType<EventSystem>();

                if (eventSystems.Length == 0)
                {
                    CameraCache.Main.gameObject.EnsureComponent<EventSystem>();
                    addedComponents = true;
                }
                else
                {
                    bool raiseWarning;

                    if (eventSystems.Length == 1)
                    {
                        raiseWarning = eventSystems[0].gameObject != CameraCache.Main.gameObject;
                    }
                    else
                    {
                        raiseWarning = true;
                    }

                    if (raiseWarning)
                    {
                        Debug.LogWarning("Found an existing event system in your scene. The Mixed Reality Toolkit requires only one, and must be found on the main camera.");
                    }
                }
            }

            if (!addedComponents)
            {
                CameraCache.Main.gameObject.EnsureComponent<EventSystem>();
            }
        }

        #endregion Instance Management

        #region MonoBehaviour Implementation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!IsInitialized &&
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ConfirmInitialized();
            }
        }
#endif // UNITY_EDITOR

        private void Awake()
        {
            if (Application.isBatchMode || !Application.isEditor)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            }

            if (!Application.isPlaying) { return; }

            if (IsInitialized && instance != this)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }

                Debug.LogWarning($"Trying to instantiate a second instance of the {nameof(MixedRealityToolkit)}. Additional Instance was destroyed");
            }
            else if (!IsInitialized)
            {
                InitializeInstance();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                EnableAllServices();
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateAllServices();
            }
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                LateUpdateAllServices();
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                FixedUpdateAllServices();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                DisableAllServices();
            }
        }

        private void OnDestroy()
        {
            DestroyAllServices();
            ClearCoreSystemCache();
            Dispose();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!Application.isPlaying) { return; }

            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var system in activeSystems)
            {
                system.Value.OnApplicationFocus(focus);
            }

            foreach (var service in registeredMixedRealityServices)
            {
                service.Item2.OnApplicationFocus(focus);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (!Application.isPlaying) { return; }

            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var system in activeSystems)
            {
                system.Value.OnApplicationPause(pause);
            }

            foreach (var service in registeredMixedRealityServices)
            {
                service.Item2.OnApplicationPause(pause);
            }
        }

        #endregion MonoBehaviour Implementation

        #region Service Container Management

        #region Registration

        /// <summary>
        /// Registers all the <see cref="IMixedRealityService"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IMixedRealityServiceConfiguration{T}"/>s.</param>
        /// <returns>True, if all configurations successfully created and registered their services.</returns>
        public static bool TryRegisterServiceConfigurations<T>(IMixedRealityServiceConfiguration<T>[] configurations) where T : IMixedRealityService
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (configuration.InstancedType.Type == null)
                {
                    anyFailed = true;
                    Debug.LogWarning($"Could not load the {configuration.Name} configuration's {nameof(configuration.InstancedType)}.");
                    continue;
                }

                if (TryCreateAndRegisterService(configuration, out var serviceInstance))
                {
                    if (configuration.Profile is IMixedRealityServiceProfile<IMixedRealityDataProvider> profile &&
                        !TryRegisterDataProviderConfigurations(profile.RegisteredServiceConfigurations, serviceInstance))
                    {
                        anyFailed = true;
                    }
                }
                else
                {
                    Debug.LogError($"Failed to start {configuration.Name}!");
                    anyFailed = true;
                }
            }

            return !anyFailed;
        }

        /// <summary>
        /// Registers all the <see cref="IMixedRealityDataProvider"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityDataProvider"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IMixedRealityServiceConfiguration{T}"/>s.</param>
        /// <param name="serviceParent">The <see cref="IMixedRealityService"/> that the <see cref="IMixedRealityDataProvider"/> will be assigned to.</param>
        /// <returns>True, if all configurations successfully created and registered their data providers.</returns>
        public static bool TryRegisterDataProviderConfigurations<T>(IMixedRealityServiceConfiguration<T>[] configurations, IMixedRealityService serviceParent) where T : IMixedRealityDataProvider
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (configuration.InstancedType.Type == null)
                {
                    anyFailed = true;
                    Debug.LogWarning($"Could not load the {configuration.Name} configuration's {nameof(configuration.InstancedType)}.");
                    continue;
                }

                if (!TryCreateAndRegisterDataProvider(configuration, serviceParent))
                {
                    Debug.LogError($"Failed to start {configuration.Name}!");
                    anyFailed = true;
                }
            }

            return !anyFailed;
        }

        /// <summary>
        /// Add a service instance to the Mixed Reality Toolkit active service registry.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="serviceInstance">Instance of the <see cref="IMixedRealityService"/> to register.</param>
        /// <returns>True, if the service was successfully registered.</returns>
        public static bool TryRegisterService<T>(IMixedRealityService serviceInstance) where T : IMixedRealityService
        {
            return TryRegisterServiceInternal(typeof(T), serviceInstance);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Mixed Reality Toolkit service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="configuration">The <see cref="IMixedRealityServiceConfiguration{T}"/> to use to create and register the service.</param>
        /// <param name="service">If successful, then the new <see cref="IMixedRealityService"/> instance will be passed back out.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterService<T>(IMixedRealityServiceConfiguration<T> configuration, out IMixedRealityService service) where T : IMixedRealityService
        {
            return TryCreateAndRegisterService<T>(
                configuration.InstancedType,
                configuration.RuntimePlatforms,
                out service,
                configuration.Name,
                configuration.Priority,
                configuration.Profile);
        }

        /// <summary>
        /// Creates a new instance of a data provider and registers it to the Mixed Reality Toolkit service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="configuration">The <see cref="IMixedRealityServiceConfiguration{T}"/> to use to create and register the data provider.</param>
        /// <param name="serviceParent">The <see cref="IMixedRealityService"/> that the <see cref="IMixedRealityDataProvider"/> will be assigned to.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterDataProvider<T>(IMixedRealityServiceConfiguration<T> configuration, IMixedRealityService serviceParent) where T : IMixedRealityDataProvider
        {
            return TryCreateAndRegisterService<T>(
                    configuration.InstancedType,
                    configuration.RuntimePlatforms,
                    out _,
                    configuration.Name,
                    configuration.Priority,
                    configuration.Profile,
                    serviceParent);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Mixed Reality Toolkit service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="service">If successful, then the new <see cref="IMixedRealityService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterService<T>(Type concreteType, out IMixedRealityService service, params object[] args) where T : IMixedRealityService
        {
            return TryCreateAndRegisterService<T>(concreteType, AllPlatforms, out service, args);
        }

        private static readonly IMixedRealityPlatform[] AllPlatforms = { new AllPlatforms() };

        /// <summary>
        /// Creates a new instance of a service and registers it to the Mixed Reality Toolkit service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IMixedRealityService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="runtimePlatforms">The runtime platform to check against when registering.</param>
        /// <param name="serviceInstance">If successful, then the new <see cref="IMixedRealityService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterService<T>(Type concreteType, IReadOnlyList<IMixedRealityPlatform> runtimePlatforms, out IMixedRealityService serviceInstance, params object[] args) where T : IMixedRealityService
        {
            serviceInstance = null;

            if (IsApplicationQuitting)
            {
                return false;
            }

            var platforms = new List<IMixedRealityPlatform>();

            Debug.Assert(ActivePlatforms.Count > 0);

            for (var i = 0; i < ActivePlatforms.Count; i++)
            {
                var activePlatform = ActivePlatforms[i].GetType();

                for (var j = 0; j < runtimePlatforms?.Count; j++)
                {
                    var runtimePlatform = runtimePlatforms[j].GetType();

                    if (activePlatform == runtimePlatform)
                    {
                        platforms.Add(runtimePlatforms[j]);
                        break;
                    }
                }
            }

            if (platforms.Count == 0)
            {
                if (runtimePlatforms == null ||
                    runtimePlatforms.Count == 0)
                {
                    Debug.LogWarning($"No runtime platforms defined for the {concreteType?.Name} service.");
                }

                // We return true so we don't raise en error.
                // Even though we did not register the service,
                // it's expected that this is the intended behavior
                // when there isn't a valid platform to run the service on.
                return true;
            }

            if (concreteType == null)
            {
                Debug.LogError($"Unable to register a service with a null concrete {typeof(T).Name} type.");
                return false;
            }

            if (Application.isEditor &&
                !CurrentBuildTargetPlatform.IsBuildTargetActive(platforms))
            {
                // We return true so we don't raise en error.
                // Even though we did not register the service,
                // it's expected that this is the intended behavior
                // when there isn't a valid build target active to run the service on.
                return true;
            }

            if (!typeof(IMixedRealityService).IsAssignableFrom(concreteType))
            {
                Debug.LogError($"Unable to register the {concreteType.Name} service. It does not implement {typeof(IMixedRealityService)}.");
                return false;
            }

            try
            {
                serviceInstance = Activator.CreateInstance(concreteType, args) as IMixedRealityService;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Debug.LogError($"Failed to register the {concreteType.Name} service: {e.InnerException?.GetType()} - {e.InnerException?.Message}\n{e.InnerException?.StackTrace}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register the {concreteType.Name} service: {e.GetType()} - {e.Message}\n{e.StackTrace}");
                return false;
            }

            if (serviceInstance == null)
            {
                Debug.LogError($"Failed to create a valid instance of {concreteType.Name}!");
                return false;
            }

            return TryRegisterServiceInternal(typeof(T), serviceInstance);
        }

        /// <summary>
        /// Internal service registration.
        /// </summary>
        /// <param name="interfaceType">The interface type for the <see cref="IMixedRealityService"/> to be registered.</param>
        /// <param name="serviceInstance">Instance of the <see cref="IMixedRealityService"/> to register.</param>
        /// <returns>True if registration is successful, false otherwise.</returns>
        private static bool TryRegisterServiceInternal(Type interfaceType, IMixedRealityService serviceInstance)
        {
            if (serviceInstance == null)
            {
                Debug.LogWarning($"Unable to add a {interfaceType.Name} service with a null instance.");
                return false;
            }

            if (!interfaceType.IsInstanceOfType(serviceInstance))
            {
                Debug.LogError($"{serviceInstance.Name} does not implement {interfaceType.Name}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(serviceInstance.Name))
            {
                Debug.LogError($"{serviceInstance.GetType().Name} doesn't have a valid name!");
                return false;
            }

            if (!CanGetService(interfaceType, serviceInstance.Name)) { return false; }

            if (GetServiceByNameInternal(interfaceType, serviceInstance.Name, out var preExistingService))
            {
                Debug.LogError($"There's already a {interfaceType.Name}.{preExistingService.Name} registered!");
                return false;
            }

            if (IsSystem(interfaceType))
            {
                activeSystems.Add(interfaceType, serviceInstance);
            }
            else if (typeof(IMixedRealityService).IsAssignableFrom(interfaceType))
            {
                registeredMixedRealityServices.Add(new Tuple<Type, IMixedRealityService>(interfaceType, serviceInstance));
            }
            else
            {
                Debug.LogError($"Unable to register {interfaceType.Name}. Concrete type does not implement an interface that derives from {nameof(IMixedRealityService)}");
                return false;
            }

            if (!isInitializing)
            {
                try
                {
                    serviceInstance.Initialize();
                    serviceInstance.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            return true;
        }

        #endregion Registration

        #region Unregistration

        /// <summary>
        /// Remove all services from the Mixed Reality Toolkit active service registry for a given type
        /// </summary>
        public static bool TryUnregisterServicesOfType<T>() where T : IMixedRealityService
        {
            return TryUnregisterServiceInternal<T>(typeof(T), string.Empty);
        }

        /// <summary>
        /// Removes a specific service with the provided name.
        /// </summary>
        /// <param name="serviceInstance">The instance of the <see cref="IMixedRealityService"/> to remove.</param>
        public static bool TryUnregisterService<T>(T serviceInstance) where T : IMixedRealityService
        {
            return TryUnregisterServiceInternal<T>(typeof(T), serviceInstance.Name);
        }

        /// <summary>
        /// Removes a specific service with the provided name.
        /// </summary>
        /// <param name="serviceName">The name of the service to be removed. (Only for runtime services) </param>
        public static bool TryUnregisterService<T>(string serviceName) where T : IMixedRealityService
        {
            return TryUnregisterServiceInternal<T>(typeof(T), serviceName);
        }

        /// <summary>
        /// Remove services from the Mixed Reality Toolkit active service registry for a given type and name
        /// Name is only supported for Mixed Reality runtime services
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">The name of the service to be removed. (Only for runtime services) </param>
        private static bool TryUnregisterServiceInternal<T>(Type interfaceType, string serviceName) where T : IMixedRealityService
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to remove null service type.");
                return false;
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                bool result = true;

                var activeServices = GetActiveServices<T>(interfaceType);

                if (activeServices.Count == 0)
                {
                    Debug.LogWarning($"No {nameof(IMixedRealityService)}s registered that implement {typeof(T).Name}.");
                    return false;
                }

                for (var i = 0; i < activeServices.Count; i++)
                {
                    result &= TryUnregisterServiceInternal<T>(interfaceType, activeServices[i].Name);
                }

                return result;
            }

            if (GetServiceByNameInternal(interfaceType, serviceName, out var serviceInstance))
            {
                var activeDataProviders = GetActiveServices<IMixedRealityDataProvider>();

                bool result = true;

                for (int i = 0; i < activeDataProviders.Count; i++)
                {
                    var dataProvider = activeDataProviders[i];

                    if (dataProvider.ParentService.Equals(serviceInstance))
                    {
                        result &= TryUnregisterService(dataProvider);
                    }
                }

                if (!result)
                {
                    Debug.LogError($"Failed to unregister all the {nameof(IMixedRealityDataProvider)}s for this {serviceInstance.Name}!");
                }

                try
                {
                    serviceInstance.Disable();
                    serviceInstance.Destroy();
                    serviceInstance.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }

                if (IsSystem(interfaceType))
                {
                    activeSystems.Remove(interfaceType);
                    return true;
                }

                Tuple<Type, IMixedRealityService> registryInstance = null;

                for (var i = 0; i < registeredMixedRealityServices.Count; i++)
                {
                    var service = registeredMixedRealityServices[i];

                    if (service.Item2.Name == serviceName)
                    {
                        registryInstance = service;
                        break;
                    }
                }

                if (registeredMixedRealityServices.Contains(registryInstance))
                {
                    registeredMixedRealityServices.Remove(registryInstance);
                    return true;
                }

                Debug.LogError($"Failed to find registry instance of [{interfaceType.Name}] \"{serviceInstance.Name}\"!");
            }

            return false;
        }

        #endregion Unregistration

        #region Multiple Service Management

        /// <summary>
        /// Enable all services in the Mixed Reality Toolkit active service registry for a given type
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be enabled.  E.G. InputSystem, BoundarySystem</typeparam>
        public static void EnableAllServicesOfType<T>() where T : IMixedRealityService
        {
            EnableAllServicesByTypeAndName(typeof(T), string.Empty);
        }

        /// <summary>
        /// Enables a single service by name.
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be enabled.  E.G. InputSystem, BoundarySystem</typeparam>
        /// <param name="serviceName"></param>
        public static void EnableService<T>(string serviceName) where T : IMixedRealityService
        {
            EnableAllServicesByTypeAndName(typeof(T), serviceName);
        }

        /// <summary>
        /// Enable all services in the Mixed Reality Toolkit active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be enabled.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">Name of the specific service</param>
        private static void EnableAllServicesByTypeAndName(Type interfaceType, string serviceName)
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to enable null service type.");
                return;
            }

            var services = new List<IMixedRealityService>();
            GetAllServicesByNameInternal(interfaceType, serviceName, ref services);

            for (int i = 0; i < services?.Count; i++)
            {
                try
                {
                    services[i].Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Disable all services in the Mixed Reality Toolkit active service registry for a given type
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be enabled.  E.G. InputSystem, BoundarySystem</typeparam>
        public static void DisableAllServiceOfType<T>() where T : IMixedRealityService
        {
            DisableAllServicesByTypeAndName(typeof(T), string.Empty);
        }

        /// <summary>
        /// Disable a single service by name.
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be enabled.  E.G. InputSystem, BoundarySystem</typeparam>
        /// <param name="serviceName">Name of the specific service</param>
        public static void DisableService<T>(string serviceName) where T : IMixedRealityService
        {
            DisableAllServicesByTypeAndName(typeof(T), serviceName);
        }

        /// <summary>
        /// Disable all services in the Mixed Reality Toolkit active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be disabled.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">Name of the specific service</param>
        private static void DisableAllServicesByTypeAndName(Type interfaceType, string serviceName)
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to disable null service type.");
                return;
            }

            var services = new List<IMixedRealityService>();
            GetAllServicesByNameInternal(interfaceType, serviceName, ref services);

            for (int i = 0; i < services?.Count; i++)
            {
                try
                {
                    services[i].Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Retrieve all services from the Mixed Reality Toolkit active service registry for a given type and an optional name
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.</typeparam>
        /// <returns>An array of services that meet the search criteria</returns>
        public static List<T> GetActiveServices<T>() where T : IMixedRealityService
        {
            return GetActiveServices<T>(typeof(T));
        }

        /// <summary>
        /// Retrieve all services from the Mixed Reality Toolkit active service registry for a given type and an optional name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <returns>An array of services that meet the search criteria</returns>
        private static List<T> GetActiveServices<T>(Type interfaceType) where T : IMixedRealityService
        {
            return GetActiveServices<T>(interfaceType, string.Empty);
        }

        /// <summary>
        /// Retrieve all services from the Mixed Reality Toolkit active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <returns>An array of services that meet the search criteria</returns>
        private static List<T> GetActiveServices<T>(Type interfaceType, string serviceName) where T : IMixedRealityService
        {
            var services = new List<T>();

            if (interfaceType == null)
            {
                Debug.LogWarning("Unable to get services with a type of null.");
                return services;
            }

            if (IsSystem(interfaceType))
            {
                foreach (var system in activeSystems)
                {
                    if (system.Key.Name == interfaceType.Name)
                    {
                        services.Add((T)system.Value);
                    }
                }
            }
            else
            {
                // If no name provided, return all services of the same type. Else return the type/name combination.
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    GetAllServicesInternal(interfaceType, ref services);
                }
                else
                {
                    GetAllServicesByNameInternal(interfaceType, serviceName, ref services);
                }
            }

            return services;
        }

        private void InitializeAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Initialize all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Initialize all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void ResetAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Reset all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Reset all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void EnableAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Enable all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Reset all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void UpdateAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Update all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Update all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void LateUpdateAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Update all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.LateUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Update all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.LateUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void FixedUpdateAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Update all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.FixedUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Update all registered runtime services
            foreach (var service in registeredMixedRealityServices)
            {
                try
                {
                    service.Item2.FixedUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void DisableAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Disable all registered runtime services in reverse priority order
            for (var i = registeredMixedRealityServices.Count - 1; i >= 0; i--)
            {
                try
                {
                    registeredMixedRealityServices[i].Item2.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Disable all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void DestroyAllServices()
        {
            // If the Mixed Reality Toolkit is not configured, stop.
            if (activeProfile == null) { return; }

            // Destroy all registered runtime services in reverse priority order
            for (var i = registeredMixedRealityServices.Count - 1; i >= 0; i--)
            {
                try
                {
                    registeredMixedRealityServices[i].Item2.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Destroy all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all registered runtime services in reverse priority order
            for (var i = registeredMixedRealityServices.Count - 1; i >= 0; i--)
            {
                try
                {
                    registeredMixedRealityServices[i].Item2.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all systems
            foreach (var system in activeSystems)
            {
                try
                {
                    system.Value.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            activeSystems.Clear();
            registeredMixedRealityServices.Clear();
        }

        #endregion Multiple Service Management

        #region Service Utilities

        /// <summary>
        /// Generic function used to interrogate the Mixed Reality Toolkit registered services registry for the existence of a service.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <remarks>
        /// Note: type should be the Interface of the system to be retrieved and not the concrete class itself.
        /// </remarks>
        /// <returns>True, there is a service registered with the selected interface, False, no service found for that interface</returns>
        public static bool IsServiceRegistered<T>() where T : IMixedRealityService
        {
            return GetService(typeof(T)) != null;
        }

        /// <summary>
        /// Generic function used to interrogate the Mixed Reality Toolkit active system registry for the existence of a core system.
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.</typeparam>
        /// <remarks>
        /// Note: type should be the Interface of the system to be retrieved and not the concrete class itself.
        /// </remarks>
        /// <returns>True, there is a system registered with the selected interface, False, no system found for that interface.</returns>
        public static bool IsSystemRegistered<T>() where T : IMixedRealityService
        {
            try
            {
                return activeSystems.TryGetValue(typeof(T), out _);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsSystem(Type concreteType)
        {
            if (concreteType == null)
            {
                Debug.LogError($"{nameof(concreteType)} is null.");
                return false;
            }

            return typeof(IMixedRealitySystem).IsAssignableFrom(concreteType);
        }

        private static void ClearCoreSystemCache()
        {
            cameraSystem = null;
            inputSystem = null;
            teleportSystem = null;
            boundarySystem = null;
            spatialAwarenessSystem = null;
            diagnosticsSystem = null;
        }

        /// <summary>
        /// Generic function used to retrieve a service from the Mixed Reality Toolkit active service registry
        /// </summary>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.
        /// *Note type should be the Interface of the system to be retrieved and not the class itself</typeparam>
        /// <returns>The instance of the service class that is registered with the selected Interface</returns>
        public static T GetService<T>(bool showLogs = true) where T : IMixedRealityService
        {
            return (T)GetService(typeof(T), showLogs);
        }

        /// <summary>
        /// Generic function used to retrieve a service from the Mixed Reality Toolkit active service registry
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.
        /// *Note type should be the Interface of the system to be retrieved and not the class itself</typeparam>
        /// <param name="service">The instance of the service class that is registered with the selected Interface</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>Returns true if the service was found</returns>
        public static bool TryGetService<T>(out T service, bool showLogs = true) where T : IMixedRealityService
        {
            service = GetService<T>(showLogs);
            return service != null;
        }

        /// <summary>
        /// Retrieve a service from the Mixed Reality Toolkit active service registry
        /// </summary>
        /// <param name="serviceName">Name of the specific service to search for</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The Mixed Reality Toolkit of the specified type</returns>
        public static T GetService<T>(string serviceName, bool showLogs = true) where T : IMixedRealityService
        {
            return (T)GetService(typeof(T), serviceName, showLogs);
        }

        /// <summary>
        /// Generic function used to retrieve a service from the Mixed Reality Toolkit active service registry.
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.
        /// *Note type should be the Interface of the system to be retrieved and not the class itself</typeparam>
        /// <param name="serviceName">Name of the specific service to search for</param>
        /// <param name="service">The instance of the service class that is registered with the selected Interface</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>Returns true if the service was found</returns>
        public static bool TryGetService<T>(string serviceName, out T service, bool showLogs = true) where T : IMixedRealityService
        {
            service = (T)GetService(typeof(T), serviceName, showLogs);
            return service != null;
        }

        /// <summary>
        /// Retrieve a service from the Mixed Reality Toolkit active service registry
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The Mixed Reality Toolkit of the specified type</returns>
        private static IMixedRealityService GetService(Type interfaceType, bool showLogs = true)
        {
            return GetService(interfaceType, string.Empty, showLogs);
        }

        /// <summary>
        /// Retrieve a service from the Mixed Reality Toolkit active service registry
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The Mixed Reality Toolkit of the specified type</returns>
        private static IMixedRealityService GetService(Type interfaceType, string serviceName, bool showLogs = true)
        {
            if (!GetServiceByNameInternal(interfaceType, serviceName, out var serviceInstance) && showLogs)
            {
                Debug.LogError($"Unable to find {(string.IsNullOrWhiteSpace(serviceName) ? interfaceType.Name : serviceName)} service.");
            }

            return serviceInstance;
        }

        /// <summary>
        /// Retrieve the first service from the registry that meets the selected type and name
        /// </summary>
        /// <param name="interfaceType">Interface type of the service being requested</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <param name="serviceInstance">return parameter of the function</param>
        private static bool GetServiceByNameInternal(Type interfaceType, string serviceName, out IMixedRealityService serviceInstance)
        {
            serviceInstance = null;

            if (!CanGetService(interfaceType, serviceName)) { return false; }

            if (IsSystem(interfaceType))
            {
                if (activeSystems.TryGetValue(interfaceType, out serviceInstance))
                {
                    if (CheckServiceMatch(interfaceType, serviceName, interfaceType, serviceInstance))
                    {
                        return true;
                    }

                    serviceInstance = null;
                }
            }
            else
            {
                var foundServices = GetActiveServices<IMixedRealityService>(interfaceType, serviceName);

                switch (foundServices.Count)
                {
                    case 0:
                        return false;
                    case 1:
                        serviceInstance = foundServices[0];
                        return true;
                    default:
                        Debug.LogError($"Found multiple instances of {interfaceType.Name}! For better results, pass the name of the service or use GetActiveServices<T>()");
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all services by type.
        /// </summary>
        /// <param name="interfaceType">The interface type to search for.</param>
        /// <param name="services">Memory reference value of the service list to update.</param>
        private static void GetAllServicesInternal<T>(Type interfaceType, ref List<T> services) where T : IMixedRealityService
        {
            GetAllServicesByNameInternal(interfaceType, string.Empty, ref services);
        }

        /// <summary>
        /// Gets all services by type and name.
        /// </summary>
        /// <param name="interfaceType">The interface type to search for.</param>
        /// <param name="serviceName">The name of the service to search for. If the string is empty than any matching <see cref="interfaceType"/> will be added to the <see cref="services"/> list.</param>
        /// <param name="services">Memory reference value of the service list to update.</param>
        private static void GetAllServicesByNameInternal<T>(Type interfaceType, string serviceName, ref List<T> services) where T : IMixedRealityService
        {
            if (!CanGetService(interfaceType, serviceName)) { return; }

            if (IsSystem(interfaceType))
            {
                if (GetServiceByNameInternal(interfaceType, serviceName, out var serviceInstance) &&
                    CheckServiceMatch(interfaceType, serviceName, interfaceType, serviceInstance))
                {
                    services.Add((T)serviceInstance);
                }
            }
            else
            {
                for (int i = 0; i < registeredMixedRealityServices.Count; i++)
                {
                    if (CheckServiceMatch(interfaceType, serviceName, registeredMixedRealityServices[i].Item1, registeredMixedRealityServices[i].Item2))
                    {
                        services.Add((T)registeredMixedRealityServices[i].Item2);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the interface type and name matches the registered interface type and service instance found.
        /// </summary>
        /// <param name="interfaceType">The interface type of the service to check.</param>
        /// <param name="serviceName">The name of the service to check.</param>
        /// <param name="registeredInterfaceType">The registered interface type.</param>
        /// <param name="serviceInstance">The instance of the registered service.</param>
        /// <returns>True, if the registered service contains the interface type and name.</returns>
        private static bool CheckServiceMatch(Type interfaceType, string serviceName, Type registeredInterfaceType, IMixedRealityService serviceInstance)
        {
            bool isNameValid = string.IsNullOrEmpty(serviceName) || serviceInstance.Name == serviceName;
            bool isInstanceValid = interfaceType == registeredInterfaceType || interfaceType.IsInstanceOfType(serviceInstance);
            return isNameValid && isInstanceValid;
        }

        private static bool CanGetService(Type interfaceType, string serviceName)
        {
            if (IsApplicationQuitting)
            {
                return false;
            }

            if (interfaceType == null)
            {
                Debug.LogError($"{serviceName} interface type is null.");
                return false;
            }

            if (!typeof(IMixedRealityService).IsAssignableFrom(interfaceType))
            {
                Debug.LogError($"{interfaceType.Name} does not implement {typeof(IMixedRealityService).Name}.");
                return false;
            }

            return true;
        }

        #endregion Service Utilities

        #endregion Service Container Management

        #region Core System Accessors

        #region Camera System

        private static IMixedRealityCameraSystem cameraSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityCameraSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityCameraSystem CameraSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsCameraSystemEnabled)
                {
                    return null;
                }

                if (cameraSystem != null)
                {
                    return cameraSystem;
                }

                cameraSystem = GetService<IMixedRealityCameraSystem>(showLogs: logCameraSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logCameraSystem = cameraSystem != null;
                return cameraSystem;
            }
        }

        private static bool logCameraSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityCameraSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateCameraSystemAsync(int timeout = 10)
        {
            try
            {
                await CameraSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Camera System

        #region Input System

        private static IMixedRealityInputSystem inputSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityInputSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityInputSystem InputSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsInputSystemEnabled)
                {
                    return null;
                }

                if (inputSystem != null)
                {
                    return inputSystem;
                }

                inputSystem = GetService<IMixedRealityInputSystem>(showLogs: logInputSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logInputSystem = inputSystem != null;
                return inputSystem;
            }
        }

        private static bool logInputSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityInputSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateInputSystemAsync(int timeout = 10)
        {
            try
            {
                await InputSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Input System

        #region Boundary System

        private static IMixedRealityBoundarySystem boundarySystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityBoundarySystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityBoundarySystem BoundarySystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsBoundarySystemEnabled)
                {
                    return null;
                }

                if (boundarySystem != null)
                {
                    return boundarySystem;
                }

                boundarySystem = GetService<IMixedRealityBoundarySystem>(showLogs: logBoundarySystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logBoundarySystem = boundarySystem != null;
                return boundarySystem;
            }
        }

        private static bool logBoundarySystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityBoundarySystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateBoundarySystemAsync(int timeout = 10)
        {
            try
            {
                await BoundarySystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Boundary System

        #region Spatial Awareness System

        private static IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealitySpatialAwarenessSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealitySpatialAwarenessSystem SpatialAwarenessSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsSpatialAwarenessSystemEnabled)
                {
                    return null;
                }

                if (spatialAwarenessSystem != null)
                {
                    return spatialAwarenessSystem;
                }

                spatialAwarenessSystem = GetService<IMixedRealitySpatialAwarenessSystem>(showLogs: logSpatialAwarenessSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logSpatialAwarenessSystem = spatialAwarenessSystem != null;
                return spatialAwarenessSystem;
            }
        }

        private static bool logSpatialAwarenessSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealitySpatialAwarenessSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateSpatialAwarenessSystemAsync(int timeout = 10)
        {
            try
            {
                await SpatialAwarenessSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Spatial Awareness System

        #region Teleport System

        private static IMixedRealityTeleportSystem teleportSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityTeleportSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityTeleportSystem TeleportSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsTeleportSystemEnabled)
                {
                    return null;
                }

                if (teleportSystem != null)
                {
                    return teleportSystem;
                }

                teleportSystem = GetService<IMixedRealityTeleportSystem>(showLogs: logTeleportSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logTeleportSystem = teleportSystem != null;
                return teleportSystem;
            }
        }

        private static bool logTeleportSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityTeleportSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateTeleportSystemAsync(int timeout = 10)
        {
            try
            {
                await TeleportSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Teleport System

        #region Networking System

        private static IMixedRealityNetworkingSystem networkingSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityNetworkingSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityNetworkingSystem NetworkingSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsNetworkingSystemEnabled)
                {
                    return null;
                }

                if (networkingSystem != null)
                {
                    return networkingSystem;
                }

                networkingSystem = GetService<IMixedRealityNetworkingSystem>(showLogs: logNetworkingSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logNetworkingSystem = networkingSystem != null;
                return networkingSystem;
            }
        }

        private static bool logNetworkingSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityNetworkingSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateNetworkingSystemAsync(int timeout = 10)
        {
            try
            {
                await NetworkingSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Networking System

        #region Diagnostics System

        private static IMixedRealityDiagnosticsSystem diagnosticsSystem = null;

        /// <summary>
        /// The current <see cref="IMixedRealityDiagnosticsSystem"/> registered with the Mixed Reality Toolkit.
        /// </summary>
        public static IMixedRealityDiagnosticsSystem DiagnosticsSystem
        {
            get
            {
                if (!IsInitialized ||
                    IsApplicationQuitting ||
                    instance.activeProfile.IsNull() ||
                   !instance.activeProfile.IsNull() && !instance.activeProfile.IsDiagnosticsSystemEnabled)
                {
                    return null;
                }

                if (diagnosticsSystem != null)
                {
                    return diagnosticsSystem;
                }

                diagnosticsSystem = GetService<IMixedRealityDiagnosticsSystem>(showLogs: logDiagnosticsSystem);
                // If we found a valid system, then we turn logging back on for the next time we need to search.
                // If we didn't find a valid system, then we stop logging so we don't spam the debug window.
                logDiagnosticsSystem = diagnosticsSystem != null;
                return diagnosticsSystem;
            }
        }

        private static bool logDiagnosticsSystem = true;

        /// <summary>
        /// Waits for the <see cref="IMixedRealityDiagnosticsSystem"/> to be valid.
        /// </summary>
        /// <param name="timeout">The number of seconds to wait for validation before timing out.</param>
        /// <returns>True, when the input system is valid, otherwise false.</returns>
        public static async Task<bool> ValidateDiagnosticsSystemAsync(int timeout = 10)
        {
            try
            {
                await DiagnosticsSystem.WaitUntil(system => system != null, timeout);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        #endregion Diagnostics System

        #endregion Core System Accessors

        #region IDisposable Implementation

        private bool disposed;

        ~MixedRealityToolkit()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Dispose the <see cref="MixedRealityToolkit"/> object.
        /// </summary>
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            GC.SuppressFinalize(this);
            OnDispose(false);
        }

        private void OnDispose(bool finalizing)
        {
            if (instance == this)
            {
                instance = null;
                searchForInstance = true;
            }
        }

        #endregion IDisposable Implementation
    }
}
