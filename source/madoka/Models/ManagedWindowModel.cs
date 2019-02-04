using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using madoka.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Mvvm;
using Reactive.Bindings;

namespace madoka.Models
{
    public enum DPIAwares
    {
        [Display(Name = "未使用")]
        Unknown = 0,

        /// <summary>
        /// 無効
        /// </summary>
        [Display(Name = "指定しない")]
        Disable = 1,

        /// <summary>
        /// アプリケーション
        /// </summary>
        [Display(Name = "アプリケーション")]
        HighDPIAware = 2,

        /// <summary>
        /// システム
        /// </summary>
        [Display(Name = "システム")]
        DPIUnAware = 3,

        /// <summary>
        /// システム（拡張）
        /// </summary>
        [Display(Name = "システム（拡張）")]
        GDIScalingUnAware = 4,
    }

    public class ManagedWindowModel : BindableBase
    {
        public ManagedWindowModel()
        {
            this.PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(this.Exe):
                    case nameof(this.Name):
                        this.SetDisplayName(this.name, this.exe);
                        if (e.PropertyName == nameof(this.Exe))
                        {
                            this.RaisePropertyChanged(nameof(this.AppIcon));
                            this.RaisePropertyChanged(nameof(this.IsExistsAppIcon));
                        }

                        break;

                    case nameof(this.DPIAware):
                    case nameof(this.IsMadokaScaling):
                        this.ScalingMode = this.IsMadokaScaling ?
                            "madoka scaling" :
                            this.DPIAware.ToDisplayName();
                        break;
                }
            };

            this.managedProcessIDList.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(this.ManagedProcessIDs));
        }

        private void SetDisplayName(
            string name,
            string exe)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.DisplayName = name;
                return;
            }

            if (!string.IsNullOrEmpty(exe))
            {
                var text = Path.GetFileNameWithoutExtension(exe);
                this.DisplayName = !string.IsNullOrEmpty(text) ? text : exe;
                return;
            }

            this.DisplayName = string.Empty;
        }

        [JsonIgnore]
        public Guid ID { get; private set; } = Guid.NewGuid();

        private string displayName;

        [JsonIgnore]
        public string DisplayName
        {
            get => this.displayName;
            set => this.SetProperty(ref this.displayName, value);
        }

        [JsonIgnore]
        public ImageSource AppIcon => NativeMethods.GetAppIcon(this.exe);

        [JsonIgnore]
        public bool IsExistsAppIcon => this.AppIcon != null;

        private string scalingMode;

        [JsonIgnore]
        public string ScalingMode
        {
            get => this.scalingMode;
            set => this.SetProperty(ref this.scalingMode, value);
        }

        private bool isEnabled = true;

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private string exe;

        [JsonProperty(PropertyName = "exe")]
        public string Exe
        {
            get => this.exe;
            set => this.SetProperty(ref this.exe, value.Replace("\"", string.Empty));
        }

        private bool isRunning;

        [JsonIgnore]
        public bool IsRunning
        {
            get => this.isRunning;
            set => this.SetProperty(ref this.isRunning, value);
        }

        private string workingDirectory;

        [JsonProperty(PropertyName = "working_directory")]
        public string WorkingDirectory
        {
            get => this.workingDirectory;
            set => this.SetProperty(ref this.workingDirectory, value);
        }

        private string arguments;

        [JsonProperty(PropertyName = "arguments")]
        public string Arguments
        {
            get => this.arguments;
            set => this.SetProperty(ref this.arguments, value);
        }

        private bool isLockLocation;

        [JsonProperty(PropertyName = "lock_location")]
        public bool IsLockLocation
        {
            get => this.isLockLocation;
            set => this.SetProperty(ref this.isLockLocation, value);
        }

        private WindowState windowState = WindowState.Normal;

        [JsonProperty(PropertyName = "window_state")]
        public WindowState WindowState
        {
            get => this.windowState;
            set => this.SetProperty(ref this.windowState, value);
        }

        private bool isRunAs = false;

        [JsonProperty(PropertyName = "runas")]
        public bool IsRunAs
        {
            get => this.isRunAs;
            set => this.SetProperty(ref this.isRunAs, value);
        }

        private DPIAwares dpiAware = DPIAwares.Disable;

        [JsonProperty(PropertyName = "dpi_aware")]
        public DPIAwares DPIAware
        {
            get => this.dpiAware;
            set => this.SetProperty(ref this.dpiAware, value);
        }

        private bool isMadokaScaling = false;

        [JsonProperty(PropertyName = "madoka_scaling")]
        public bool IsMadokaScaling
        {
            get => this.isMadokaScaling;
            set => this.SetProperty(ref this.isMadokaScaling, value);
        }

        private double madokaScale = 1.0d;

        [JsonProperty(PropertyName = "madoka_scale")]
        public double MadokaScale
        {
            get => this.madokaScale;
            set => this.SetProperty(ref this.madokaScale, Math.Round(value, 2));
        }

        private NativeMethods.PROCESS_DPI_AWARENESS processDPIAwareness = NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;

        [JsonIgnore]
        public NativeMethods.PROCESS_DPI_AWARENESS ProcessDPIAwareness
        {
            get => this.processDPIAwareness;
            set => this.SetProperty(ref this.processDPIAwareness, value);
        }

        private int x;

        [JsonProperty(PropertyName = "x")]
        public int X
        {
            get => this.x;
            set => this.SetProperty(ref this.x, value);
        }

        private int y;

        [JsonProperty(PropertyName = "y")]
        public int Y
        {
            get => this.y;
            set => this.SetProperty(ref this.y, value);
        }

        private int w;

        [JsonProperty(PropertyName = "w")]
        public int W
        {
            get => this.w;
            set => this.SetProperty(ref this.w, value);
        }

        private int h;

        [JsonProperty(PropertyName = "h")]
        public int H
        {
            get => this.h;
            set => this.SetProperty(ref this.h, value);
        }

        private bool isSetLocation = false;

        [JsonProperty(PropertyName = "set_location")]
        public bool IsSetLocation
        {
            get => this.isSetLocation;
            set => this.SetProperty(ref this.isSetLocation, value);
        }

        private bool isSetSize = false;

        [JsonProperty(PropertyName = "set_size")]
        public bool IsSetSize
        {
            get => this.isSetSize;
            set => this.SetProperty(ref this.isSetSize, value);
        }

        private string group;

        [JsonProperty(PropertyName = "group")]
        public string Group
        {
            get => this.group;
            set => this.SetProperty(ref this.group, value);
        }

        [JsonIgnore]
        public ManagedWindowGroupModel Parrent { get; set; }

        private ObservableCollection<int> managedProcessIDList = new ObservableCollection<int>();

        [JsonIgnore]
        public ObservableCollection<int> ManagedProcessIDList
        {
            get => this.managedProcessIDList;
            set
            {
                this.managedProcessIDList.Clear();
                this.managedProcessIDList.AddRange(value);
            }
        }

        [JsonIgnore]
        public string ManagedProcessIDs => string.Join(", ", this.ManagedProcessIDList);

        #region Methods

        public async Task<bool> Run()
        {
            if (!this.isEnabled)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.exe) ||
                !File.Exists(this.exe))
            {
                return false;
            }

            var directory = string.IsNullOrEmpty(this.workingDirectory) ?
                Path.GetDirectoryName(this.exe) :
                this.workingDirectory;

            await Task.Run(async () =>
            {
                var pi = new ProcessStartInfo()
                {
                    FileName = this.exe,
                    Arguments = this.arguments,
                    WorkingDirectory = workingDirectory,
                    Verb = this.isRunAs ? "RunAs" : null,
                };

                var p = Process.Start(pi);
                await Task.Delay(10);

                try
                {
                    p.WaitForInputIdle();
                }
                catch (InvalidOperationException)
                {
                    await Task.Delay(200);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.IsRunning = true;
                });

                await Task.Delay(TimeSpan.FromSeconds(0.1));
                await this.SetWindowRect();
            });

            return true;
        }

        private int lastDPIAwarenessDetectedProcessID = 0;

        public async Task GetProcessDPIAwareness()
        {
            await Task.Run(async () =>
            {
                if (!this.ManagedProcessIDList.Any())
                {
                    return;
                }

                var id = this.ManagedProcessIDList.Last();

                if (this.lastDPIAwarenessDetectedProcessID != id)
                {
                    this.lastDPIAwarenessDetectedProcessID = id;
                    var state = NativeMethods.GetDPIState((uint)id);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        this.ProcessDPIAwareness = state;
                    });
                }
            });
        }

        public async Task GetWindowInfo()
        {
            if (string.IsNullOrEmpty(this.exe) ||
                !File.Exists(this.exe))
            {
                return;
            }

            var t1 = Task.Run(async () =>
            {
                var key = $@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
                var name = $"{this.exe}";
                var reg = Registry.CurrentUser.OpenSubKey(key, true);

                var value = (string)reg?.GetValue(name) ?? string.Empty;
                if (string.IsNullOrEmpty(value))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        this.IsRunAs = false;
                        this.DPIAware = DPIAwares.Disable;
                    });

                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.IsRunAs = value.ContainsIgnoreCase("RUNASADMIN");
                });

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (value.ContainsIgnoreCase("HIGHDPIAWARE"))
                    {
                        this.DPIAware = DPIAwares.HighDPIAware;
                    }
                    else if (value.ContainsIgnoreCase("DPIUNAWARE"))
                    {
                        this.DPIAware = DPIAwares.DPIUnAware;
                    }
                    else if (value.ContainsIgnoreCase("GDIDPISCALING DPIUNAWARE"))
                    {
                        this.DPIAware = DPIAwares.GDIScalingUnAware;
                    }
                    else
                    {
                        this.DPIAware = DPIAwares.Disable;
                    }
                });
            });

            var t2 = Task.Run(async () =>
            {
                var handle =
                    Process.GetProcessesByName(Path.GetFileNameWithoutExtension(this.exe))?
                    .FirstOrDefault()?
                    .MainWindowHandle;

                if (!handle.HasValue ||
                    handle.Value == IntPtr.Zero)
                {
                    return;
                }

                var rect = new NativeMethods.RECT();
                NativeMethods.GetWindowRect(handle.Value, ref rect);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.X = rect.Left;
                    this.Y = rect.Top;
                    this.W = rect.Width;
                    this.H = rect.Height;
                });
            });

            await Task.WhenAll(t1, t2);
        }

        public async Task<bool> SetRegistry()
        {
            if (string.IsNullOrEmpty(this.exe) ||
                !File.Exists(this.exe))
            {
                return false;
            }

            return await Task.Run(() =>
            {
                var key = $@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
                var name = $"{this.exe}";
                var reg = Registry.CurrentUser.OpenSubKey(key, true);

                var value = (string)reg?.GetValue(name) ?? string.Empty;

                if (!this.isRunAs &&
                    this.DPIAware == DPIAwares.Disable)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        reg.DeleteValue(name, false);
                        return true;
                    }

                    value = value.Replace("RUNASADMIN", string.Empty);
                    value = value.Replace("GDIDPISCALING DPIUNAWARE", string.Empty);
                    value = value.Replace("DPIUNAWARE", string.Empty);
                    value = value.Replace("HIGHDPIAWARE", string.Empty);
                    value = value.Trim();

                    if (!string.IsNullOrEmpty(value))
                    {
                        reg.SetValue(name, value);
                    }
                    else
                    {
                        reg.DeleteValue(name, false);
                    }

                    return true;
                }

                value = value.Replace("RUNASADMIN", string.Empty);
                value = value.Replace("GDIDPISCALING DPIUNAWARE", string.Empty);
                value = value.Replace("DPIUNAWARE", string.Empty);
                value = value.Replace("HIGHDPIAWARE", string.Empty);
                value = value.Trim();

                var addValues = new List<string>(new[] { value });
                if (this.isRunAs)
                {
                    addValues.Add("RUNASADMIN");
                }

                switch (this.dpiAware)
                {
                    case DPIAwares.HighDPIAware:
                        addValues.Add("HIGHDPIAWARE");
                        break;

                    case DPIAwares.DPIUnAware:
                        addValues.Add("DPIUNAWARE");
                        break;

                    case DPIAwares.GDIScalingUnAware:
                        addValues.Add("GDIDPISCALING DPIUNAWARE");
                        break;
                }

                value = string.Join(" ", addValues);
                reg.SetValue(name, value.Trim());

                return true;
            });
        }

        public async Task<bool> SetWindowRect(
            bool isAlways = false)
        {
            if (!this.isEnabled)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.exe) ||
                !File.Exists(this.exe))
            {
                return false;
            }

            return await Task.Run(() =>
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(this.exe));
                if (processes == null ||
                    !processes.Any())
                {
                    this.ManagedProcessIDList.Clear();
                    return false;
                }

                var applied = false;
                foreach (var p in processes)
                {
                    if (p.HasExited)
                    {
                        continue;
                    }

                    if (!isAlways)
                    {
                        if (this.ManagedProcessIDList.Contains(p.Id))
                        {
                            continue;
                        }
                    }

                    var handle = p.MainWindowHandle;
                    if (handle == IntPtr.Zero)
                    {
                        continue;
                    }

                    var flag = (uint)(NativeMethods.SWPFlags.NOACTIVATE | NativeMethods.SWPFlags.NOZORDER);

                    if (!this.isSetLocation)
                    {
                        flag |= (uint)NativeMethods.SWPFlags.NOREPOSITION;
                    }

                    if (!this.isSetSize)
                    {
                        flag |= (uint)NativeMethods.SWPFlags.NOSIZE;
                    }

                    if (this.isSetLocation ||
                        this.isSetSize)
                    {
                        NativeMethods.SetWindowPos(
                            handle,
                            IntPtr.Zero,
                            this.x,
                            this.y,
                            this.w,
                            this.h,
                            flag);

                        applied = true;
                    }

                    var cmd = 0;
                    switch (this.windowState)
                    {
                        case WindowState.Minimized:
                            cmd = (int)NativeMethods.ShowState.SW_MINIMIZE;
                            break;

                        case WindowState.Maximized:
                            cmd = (int)NativeMethods.ShowState.SW_MAXIMIZE;
                            break;
                    }

                    if (cmd != 0)
                    {
                        NativeMethods.ShowWindowAsync(handle, cmd);
                    }

                    if (!this.ManagedProcessIDList.Contains(p.Id))
                    {
                        this.ManagedProcessIDList.Add(p.Id);
                    }
                }

                return applied;
            });
        }

        #endregion Methods
    }
}
