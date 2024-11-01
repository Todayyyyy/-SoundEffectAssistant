# SoundEffectAssistant
用win窗体程序开发的音效助手。音效助手普遍应用于直播行业，主播或者助理通过音效助手播放音效以及音乐。
借助NAudio库进行开发。
## 一、音频功能的开发

核心功能的开发，利用NAudio.dll组件进行开发

### 委托

```c#
delegate double Cal(double x,double y);

static void Test(Cal f)
{
    Console.Write("请输入X:");
    double x=Convert.ToDouble(Console.ReadLine());
    Console.write("请输入Y:");
    double y= Convert.ToDouble(Console. ReadLine());//委托方法工作,怎么工作由具体委托决定
    double result =f(x, y);
    Console.writeLine("x: {0}与Y:{1}委托方法计算结果为:{2}"，x,y,result);
}

static void Main(string[] args)
{
	Test(new Cal(Add));
    Test(Add);
}

private void double Add(double x,double y){
    //...
}

```

将不变的进行封装，将变的隔离起来，让代码更易于维护。

> ![image-20241029134923881](C:\Users\WYCM\Desktop\Typora\temp\weituo)

#### 使用步骤

定义委托

实例化委托（赋予方法）

使用委托（输入参数）

#### 观察者模式

一种事件触发的模式。当定义某个委托之后，其他的类的方法可以订阅这个委托，然后通过这个委托，来统一触发一系列事件。

```c#
MyEvent?.Invoke(this, e); // 检查是否有任何订阅者，然后触发事件
```



> ![image-20241029141958516](C:\Users\WYCM\Desktop\Typora\temp\weituo2)
>
> ![image-20241029142154108](C:\Users\WYCM\Desktop\Typora\temp\weituo3)

#### 委托类型/委托事件

定义：

```c#
public static event OnGameOver onGameOver;
```

事件委托和常规委托相同，只是事件委托只能从自己的类中进行调用，其他脚本只能进行订阅或者取消，但是不能触发、更改。这样可以防止其他脚本执行相关操作。



#### 两种预定的事件泛型

无返回值：

```c#
Action<T1,T2,T3>
```

有返回值：

```c#
Func<T1,T2,T3>
```



#### Lambda表达式

```c#
() =>{

//函数

}
```

lambda表达式要和委托，事件联合使用。

例如：

```c#
Func<int , int> func = x => x+1;

Func<int , int> func;
public int fx(int x){
    return x+1;
}

```

再例如：

```c#
Action<string> action =msg =>Console.WriteLine(msg);

Action<string> action string msg =>Console.WriteLine(msg);
```

#### 事件绑定

```c#
button.Click += (s, e) => Button_Click(button);
```

```c#
public event EventHandler Click{//}
//delegate void System.EventHandler(object sender, System.EventArgs e)表示将用于处理不具有事件数据的事件的方法。
```

以下两种表述都是错误的：

```c#
button.Click += Button_Click;
```

```c#
button.Click += Button_Click(Button button);
```

因为：

`Button_Click` 方法的签名与 `EventHandler` 委托不匹配，使用 `button.Click += Button_Click;` 将会导致编译错误。

`EventHandler` 的签名是：

```
csharp


复制代码
void EventHandler(object sender, EventArgs e);
```

假设 `Button_Click` 方法的签名是这样的：

```
csharp复制代码private void Button_Click(Button button)
{
    // 处理逻辑
}
```

那么在这种情况下，`Button_Click` 的签名不匹配 `EventHandler` 委托，因此您不能直接使用方法组来订阅事件。编译器会报错，因为它找不到与 `EventHandler` 委托匹配的重载。

### Hook

略...

### 音频设备识别与获取和切换

通过类AudioDeviceManager来实现音频设备的识别。

```C#
using System.Collections.Generic;
using NAudio.Wave;

public class AudioDeviceManager
{
    public List<WaveOutCapabilities> GetAudioDevices()
    {
        List<WaveOutCapabilities> devices = new List<WaveOutCapabilities>();
        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            devices.Add(WaveOut.GetCapabilities(i));
        }
        return devices;
    }
}

```

#### public List<WaveOutCapabilities> GetAudioDevices()方法

通常用于获取系统中可用的音频设备列表，并返回一个包含 `WaveOutCapabilities` 对象的列表。每个 `WaveOutCapabilities` 对象描述了一个音频输出设备的特性，比如：

- 设备名称
- 支持的声道数
- 采样率
- 支持的位深度

#### `WaveOut.DeviceCount` 

是一个静态属性，用于获取系统中可用的音频输出设备的数量。这个属性返回一个**整数**，表示当前可以用于音频播放的设备总数。可以用它来遍历每个设备，进而获取设备的详细信息。

#### WaveOut.GetCapabilities(i)

是一个静态方法，用于获取指定索引 `i` 的音频输出设备的能力（`WaveOutCapabilities`）。该方法返回一个 `WaveOutCapabilities` 对象，其中包含有关设备的详细信息，例如：

- **设备名称**：音频设备的描述性名称。
- **支持的声道数**：设备支持的声道（单声道或立体声）数量。
- **最大采样率**：设备支持的最高采样率。
- **最大位深度**：设备支持的最高位深度（如16位、24位等）。

#### 实现步骤

```c#
private void LoadAudioDevices()
{
    var devices = audioDeviceManager.GetAudioDevices();
    foreach (var device in devices)
    {
        comboBoxDevices.Items.Add(device.ProductName);
    }
    if (comboBoxDevices.Items.Count > 0)
    {
        comboBoxDevices.SelectedIndex = 0;
    }
}
```

`LoadAudioDevices` 方法的功能是加载系统中的音频设备并将它们添加到下拉框（`comboBoxDevices`）中。具体步骤如下：

1. **获取音频设备列表**：调用 `audioDeviceManager.GetAudioDevices()` 方法，获取可用的音频设备列表。
2. **添加设备到下拉框**：使用 `foreach` 循环遍历设备列表，将每个设备的名称（`device.ProductName`）添加到 `comboBoxDevices.Items` 中。
3. **选择第一个设备**：如果下拉框中有设备项（`Items.Count` 大于 0），将 `comboBoxDevices.SelectedIndex` 设置为 0，默认选择第一个设备。
4. 注意事项：
   - 确保 `audioDeviceManager` 已正确初始化。
   - 如果需要处理没有可用设备的情况，可以考虑在下拉框中显示一条提示信息。

#### 音频设备的切换

```c#
private void comboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
{
    if (waveOut != null)
    {
        waveOut.Stop();
        waveOut.Dispose();
        waveOut = null;
    }

    if (audioFileReader != null)
    {
        audioFileReader.Dispose();
        audioFileReader = null;
    }

    if (!string.IsNullOrEmpty(currentFilePath))
    {
        waveOut = new WaveOutEvent
        {
            DeviceNumber = comboBoxDevices.SelectedIndex,
            Volume = volumeSlider.Volume
        };
        audioFileReader = new AudioFileReader(currentFilePath);
        waveOut.Init(audioFileReader);

        if (isPlaying)
        {
            waveOut.Play();
        }
    }
}
```

这个 `comboBoxDevices_SelectedIndexChanged` 方法的功能是处理用户在设备选择下拉框（`comboBoxDevices`）中更改选定设备时的逻辑。具体步骤如下：

1. **停止并释放现有的 `waveOut`**：如果 `waveOut` 不为空，先停止播放并释放其资源。
2. **释放音频文件读取器**：如果 `audioFileReader` 不为空，释放其资源。
3. **检查当前文件路径**：如果 `currentFilePath` 不为空，则进行下一步：
4. **创建新的 `WaveOutEvent` 实例**：
   - 设置 `DeviceNumber` 为所选设备的索引。
   - 将音量设置为滑块（`volumeSlider.Volume`）的值。
5. **初始化 `AudioFileReader`**：使用当前文件路径创建新的 `AudioFileReader` 实例，并用其初始化 `waveOut`。
6. **播放音频**：如果当前状态为播放（`isPlaying` 为真），则调用 `waveOut.Play()` 开始播放。

注意事项：

- 确保在选择新设备时，正确管理资源，防止内存泄漏。
- 如果 `currentFilePath` 可能为空，考虑在此处添加错误处理逻辑。





### 音频管理系统

#### 音频文件播放的实现步骤

**初始化**：在构造函数或初始化方法中实例化 `waveOut`。

**加载音频数据**：将音频数据加载到 `waveOut` 中。通常你会使用 `AudioFileReader` 来读取音频文件。

```c#
var audioFile = new AudioFileReader("path/to/audio/file.mp3");
waveOut.Init(audioFile);
```

**播放音频；**

**控制音频**；

**释放资源**：在不再需要时，确保调用 `Dispose()` 方法释放（waveout和audiofile）资源。



#### `WaveStream` 

是 NAudio 中的一个基类，用于表示音频数据流，它可以用于处理各种音频格式。

`WaveStream` 的常见用途：

1. **加载音频文件**：可以通过继承 `WaveStream` 的类（如 `AudioFileReader`）来加载音频文件。
2. **音频播放**：`WaveStream` 可以与 `WaveOut` 或 `WaveOutEvent` 等输出设备配合使用，以实现音频播放。
3. **处理流数据**：可以用于处理实时音频流，比如录音或网络音频流。

#### InitializeButtons()

```c#
private void InitializeButtons()
{
    foreach (var button in buttonAudioMapping.Keys)
    {
        if (button != ButtonPlay)
        {
            button.Click += (s, e) => Button_Click(button);
            button.MouseUp += Button_MouseUp;
        }
    }
```

**点击事件**：

```
button.Click += (s, e) => Button_Click(button);
```

- 这行代码为 `button` 的 `Click` 事件添加了一个匿名方法（lambda 表达式）。当按钮被点击时，这个匿名方法会被调用。
- 在这个方法中，调用了 `Button_Click(button)`，将当前按钮作为参数传递。这样，你可以在 `Button_Click` 方法中处理点击事件，使用特定于该按钮的逻辑。

> Lambda语法：
>
> (参数列表)=>
>
> {
>
> ​	//函数体
>
> }
>
> ​	//"=>"意思为goes to。





#### 音频播放管理系统

```c#
private byte[] ReadStreamToByteArray(UnmanagedMemoryStream stream)
{
    using (var memoryStream = new MemoryStream())
    {
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}

private void InitializeButtonAudioMapping()
{
    buttonAudioMapping = new Dictionary<Button, MemoryStream>
    {
        //笑声
        { ButtonPlayPause, new MemoryStream(ReadStreamToByteArray(Properties.Resources.SFX_001)) },// 继续为其他按钮设置音频文件路径
    };
}
private void InitializeButtons()
{
    foreach (var button in buttonAudioMapping.Keys)
    {
        if (button != ButtonPlay)
        {
            button.Click += (s, e) => Button_Click(button);
            button.MouseUp += Button_MouseUp;
        }
    }
}

private void Button_Click(Button button)
{
    HandleButtonClick(button);
}

private void HandleButtonClick(Button button)
{
    if (!buttonAudioMapping.TryGetValue(button, out MemoryStream audioStream))
    {
        MessageBox.Show("音频文件未定义");
        return;
    }

    if (waveStream == null || waveOut == null || isPlaying)
    {
        StartPlayback(audioStream);
        ButtonPlay.BackgroundImage = Properties.Resources.pause;
    }
    else
    {
        StartPlayback(audioStream);
        ButtonPlay.BackgroundImage = Properties.Resources.pause;
    }
}

private void CleanUp()
{
    if (waveOut != null)
    {
        // 确保事件处理程序被解绑
        waveOut.PlaybackStopped -= OnPlaybackStopped;
        waveOut.Stop();
        waveOut.Dispose();
        waveOut = null;
    }

    if (audioFileReader != null)
    {
        audioFileReader.Dispose();
        audioFileReader = null;
    }
}

private void ButtonPlay_Click(object sender, EventArgs e)
{
    if (waveOut == null || waveStream == null)
    {
        if (buttonAudioMapping.TryGetValue(ButtonPlay, out MemoryStream audioStream))
        {
            StartPlayback(audioStream);
            ButtonPlay.BackgroundImage = Properties.Resources.pause;
        }
    }
    else
    {
        if (waveOut.PlaybackState == PlaybackState.Playing)
        {
            waveOut.Pause();
            isPlaying = false;
            ButtonPlay.BackgroundImage = Properties.Resources.play;
        }
        else
        {
            waveOut.Play();
            isPlaying = true;
            ButtonPlay.BackgroundImage = Properties.Resources.pause;
        }
    }
}

private void StartPlayback(MemoryStream audioStream)
{
    CleanUp();

    waveOut = new WaveOutEvent
    {
        DeviceNumber = comboBoxDevices.SelectedIndex
    };

    // 重置流位置
    audioStream.Seek(0, SeekOrigin.Begin);

    // 使用 WaveFileReader
    waveStream = new WaveFileReader(audioStream);
    waveOut.Init(waveStream);
    waveOut.PlaybackStopped += OnPlaybackStopped;

    waveOut.Play();
    isPlaying = true;

    // 确保初始化时更新进度条
    trackBarProgress.Maximum = (int)waveStream.TotalTime.TotalSeconds;
}

private void ChangeAudioFile(Button button)
{
    // 设置只允许选择 .wav 文件
    openFileDialog.Filter = "WAV files (*.wav)|*.wav";

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        string newFilePath = openFileDialog.FileName;

        // 检查文件扩展名是否为 .wav（如果需要进一步验证）
        if (System.IO.Path.GetExtension(newFilePath).ToLower() != ".wav")
        {
            MessageBox.Show("只能选择 .wav 格式的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        button.Text = System.IO.Path.GetFileNameWithoutExtension(newFilePath);

        // 加载音频文件为 MemoryStream
        byte[] audioBytes = System.IO.File.ReadAllBytes(newFilePath);
        System.IO.MemoryStream audioStream = new System.IO.MemoryStream(audioBytes);

        // 更新 buttonAudioMapping 字典
        if (buttonAudioMapping.ContainsKey(button))
        {
            buttonAudioMapping[button].Dispose(); // 释放旧的 MemoryStream
            buttonAudioMapping[button] = audioStream;
        }
        else
        {
            buttonAudioMapping.Add(button, audioStream);
        }

        // 更新 appSettings 中的音频文件路径
        appSettings.ButtonAudioFilePaths[button.Name] = newFilePath;

        // 自动保存设置
        settingsManager.SaveSettings(appSettings);
    }

}
private void OnPlaybackStopped(object sender, StoppedEventArgs e)
{
    isPlaying = false;

    // 检查是否正在播放其他音频
    if (waveOut == null || waveOut.PlaybackState != PlaybackState.Playing)
    {
        ButtonPlay.BackgroundImage = Properties.Resources.play; // 播放图片
    }
}
```

##### 1.**音频文件加载和映射**

- `ReadStreamToByteArray` 方法将 `UnmanagedMemoryStream` 读取到字节数组。
- `InitializeButtonAudioMapping` 方法将音频文件与按钮关联，使用 `MemoryStream` 存储每个音频文件的内容。
- 采用这样的方式储存音频数据原因：

​	**内存管理**：通过将音频流读取到 `MemoryStream` 中，可以更灵活地管理内存。`MemoryStream` 允许在内存中对音频数据进行快速读写操作，而不需要频繁地访问磁盘文件。

​	**易于处理**：在播放音频时，将其存储在内存中可以加快访问速度，因为它避免了每次播放时都去读取文件系统的延迟。这对于用户体验非常重要。

​	**数据封装**：使用 `ReadStreamToByteArray` 方法将音频文件的流转换为字节数组，使得音频文件可以以更加统一的方式存储和使用。这使得将多个按钮与不同的音频文件关联变得简单。

​	**资源整合**：`Properties.Resources` 可以将资源嵌入到程序中，方便管理和访问。将其转换为 `MemoryStream` 使得你可以在运行时直接从嵌入的资源读取数据。

​	**避免硬编码路径**：直接使用嵌入的资源而不是依赖于文件路径，减少了在文件管理上的复杂性，同时也提高了程序的可移植性。

`UnmanagedMemoryStream` 是 C# 中的一个类，通常用于处理非托管内存中的数据。与常规的 `MemoryStream` 不同，`UnmanagedMemoryStream` 主要用于读取和写入存储在非托管内存中的字节数据。这种类型的流适合处理那些不由 .NET 垃圾回收器管理的内存，例如：

1. **与 P/Invoke 交互**：当你需要调用本地 API（如 Windows API）时，通常需要在非托管内存中操作数据。
2. **处理大数据**：在处理大文件或数据块时，可以使用非托管内存来提高性能，因为它可以减少内存分配的开销。
3. **性能优化**：由于它直接在非托管内存中操作，可能会在某些情况下提供更好的性能，特别是在与硬件接口或其他语言的代码交互时。

`UnmanagedMemoryStream` 通常会在以下情况下使用：

- 从操作系统或其他低级 API 中获取数据，并在 C# 中处理。
- 在 C# 与 C/C++ 等语言的互操作中，用于传递指向非托管内存的指针。

例子

一个简单的例子是，你可能会在调用某个非托管函数之前，先在非托管内存中分配空间，然后使用 `UnmanagedMemoryStream` 来读取或写入数据。

总之，`UnmanagedMemoryStream` 是一个强大的工具，适用于需要直接操作非托管内存的场景。

> **非托管内存**是指不由 .NET 垃圾回收器（GC）管理的内存。这种内存通常由应用程序直接分配和释放，主要用于与底层系统或其他语言（如 C/C++）交互。以下是一些关于非托管内存的关键点：
>
> 1. **手动管理**：在使用非托管内存时，开发者需要手动分配和释放内存，使用诸如 `Marshal.AllocHGlobal` 和 `Marshal.FreeHGlobal` 等方法。
> 2. **性能优化**：非托管内存可以用于处理性能要求较高的场景，比如大型数据结构或高频率的数据交互，因为它可以减少内存分配和回收的开销。
> 3. **与系统 API 交互**：许多系统级 API 和库（尤其是 C/C++ 编写的）需要使用非托管内存，因为它们不理解 .NET 的内存管理模型。
> 4. **数据共享**：非托管内存可以用于在不同语言或模块之间共享数据，例如在 C# 和 C/C++ 代码之间传递复杂数据结构。
> 5. **可能的内存泄漏**：由于需要手动管理，开发者必须小心以避免内存泄漏或未定义行为，这些都是在非托管内存中比较常见的问题。
>
> 在使用非托管内存时，确保了解其管理方式是非常重要的，以确保程序的稳定性和性能。



> **`out` 关键字**：
>
> - 表示这是一个输出参数。在方法内部，必须对其进行赋值。
> - 调用方法时，不需要在外部声明该变量，只需声明它为 `out` 类型即可。
>
> **用途**：
>
> - `out` 参数通常用于从方法中返回多个值。在 C# 中，一个方法只能返回一个值，但可以通过 `out` 参数返回额外的信息或结果。
> - 在你的例子中，`audioStream` 是一个 `MemoryStream` 对象，可能用于返回音频数据的流。



**数据流的转换**

```c#
private byte[] ReadStreamToByteArray(UnmanagedMemoryStream stream)
{
    using (var memoryStream = new MemoryStream())
    {
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
```

这段代码的作用是将一个 `UnmanagedMemoryStream` 对象的内容复制到一个新的 `MemoryStream` 中，并返回其字节数组。以下是逐步解析：

1. **`using` 语句**：确保 `memoryStream` 在使用完后自动释放资源，即使在发生异常时也能保证资源得到释放。
2. **`stream.CopyTo(memoryStream)`**：将 `UnmanagedMemoryStream` 的内容复制到 `memoryStream` 中。`CopyTo` 方法会从源流读取数据并写入到目标流。
3. **`memoryStream.ToArray()`**：将 `memoryStream` 中的内容转换为字节数组并返回。

> `using` 语句在 C# 中用于确保对象在使用完后能够自动释放其占用的资源。主要有以下几个作用：
>
> 1. **自动释放资源**：`using` 语句会在代码块结束时自动调用对象的 `Dispose` 方法，释放资源。这在处理需要显式释放资源的对象（如文件流、数据库连接、图形资源等）时特别重要。
> 2. **简化代码**：通过使用 `using`，你不需要显式调用 `Dispose`，这样可以减少代码的复杂性，并提高可读性。
> 3. **异常安全**：即使在 `using` 语句块中发生异常，`Dispose` 仍然会被调用，确保资源得到正确释放，从而避免内存泄漏。
>
> ### 示例
>
> 以下是一个简单的示例，展示了如何使用 `using` 语句：
>
> ```
> csharp复制代码using (var fileStream = new FileStream("example.txt", FileMode.Open))
> {
>     // 进行文件操作
> } // 这里会自动调用 fileStream.Dispose()
> ```
>
> 在这个例子中，`fileStream` 会在 `using` 块结束后自动关闭，无需手动调用 `fileStream.Close()` 或 `fileStream.Dispose()`。
>
> ### 注意事项
>
> - `using` 语句只能用于实现了 `IDisposable` 接口的对象。
> - 如果对象在 `using` 块之外的地方仍然被引用，确保对象不会被重复释放。

Dcitionary.Keys返回的是，所有key的集合Dictionary2，值的类型和Dictionary中key的类型一致。



##### 2.音频播放逻辑实现

button_click → handle(判断能否播放) → cleanup(释放上一次播放的内存)  → startplay(开始播放)

音频储存方式变化：audiostream                                                                      → wavestream(可以利用wave的play功能)

**`out` 关键字**：

- 表示这是一个输出参数。在<u>方法内部</u>，必须对其进行赋值。
- 调用方法时，<u>不需要在外部声明该变量</u>，只需声明它为 `out` 类型即可。

**用途**：

- `out` 参数通常用于从方法中返回多个值。在 C# 中，一个方法只能返回一个值，但可以通过 `out` 参数返回额外的信息或结果。
- 在例子中，`audioStream` 是一个 `MemoryStream` 对象，可能用于返回音频数据的流。

在这个例子中，`TryGetValue` 方法尝试从文件中读取音频流，并通过 `out` 参数返回 `MemoryStream` 对象。如果成功，调用者可以使用这个流；如果失败，流会被设置为 `null`。

> ### `WaveFileReader` 与 `AudioFileReader` 的区别
>
> 1. **`WaveFileReader`**：
>    - 专门用于读取 `.wav` 文件格式。
>    - 提供对 WAV 文件的解析，包括文件头、采样率、通道数等信息。
>    - 适用于需要处理纯 WAV 数据的场景。
> 2. **`AudioFileReader`**：
>    - 更通用，能够处理多种音频格式（如 WAV、MP3、AAC 等）。
>    - 内部会根据文件格式自动解码并转换为 PCM 数据。
>    - 适合需要处理不同音频格式的场景。
>
> ### 使用场景
>
> - 如果你只处理 WAV 文件，使用 `WaveFileReader` 可以更高效，因为它针对这种格式进行了优化。
> - 如果你的应用需要支持多种音频格式，使用 `AudioFileReader` 会更方便，因为它可以自动处理多种类型的音频数据。



##### 3.播放文件选择

```c#
private void ChangeAudioFile(Button button)
{
    // 设置只允许选择 .wav 文件
    openFileDialog.Filter = "WAV files (*.wav)|*.wav";

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        string newFilePath = openFileDialog.FileName;

        // 检查文件扩展名是否为 .wav（如果需要进一步验证）
        if (System.IO.Path.GetExtension(newFilePath).ToLower() != ".wav")
        {
            MessageBox.Show("只能选择 .wav 格式的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        button.Text = System.IO.Path.GetFileNameWithoutExtension(newFilePath);

        // 加载音频文件为 MemoryStream
        byte[] audioBytes = System.IO.File.ReadAllBytes(newFilePath);
        System.IO.MemoryStream audioStream = new System.IO.MemoryStream(audioBytes);

        // 更新 buttonAudioMapping 字典
        if (buttonAudioMapping.ContainsKey(button))
        {
            buttonAudioMapping[button].Dispose(); // 释放旧的 MemoryStream
            buttonAudioMapping[button] = audioStream;
        }
        else
        {
            buttonAudioMapping.Add(button, audioStream);
        }

        // 更新 appSettings 中的音频文件路径
        appSettings.ButtonAudioFilePaths[button.Name] = newFilePath;

        // 自动保存设置
        settingsManager.SaveSettings(appSettings);
    }
}
```

**设置文件对话框过滤器**：

```c#
openFileDialog.Filter = "WAV files (*.wav)|*.wav";
```

这行代码设置文件选择对话框只允许选择 `.wav` 文件。

**显示对话框并检查用户选择**：

```c#
if (openFileDialog.ShowDialog() == DialogResult.OK)
```

显示文件对话框，用户选择文件后，如果点击“确定”，则继续执行后续代码。

**获取选择的文件路径**：

```c#
string newFilePath = openFileDialog.FileName;
```

**检查文件扩展名**（可选）：

```c#
if (System.IO.Path.GetExtension(newFilePath).ToLower() != ".wav")
{
    MessageBox.Show("只能选择 .wav 格式的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return;
}
```

如果用户选择的文件不是 `.wav` 格式，则弹出错误提示，并退出方法。

**更新按钮文本**：

```c#
button.Text = System.IO.Path.GetFileNameWithoutExtension(newFilePath);
```

将按钮的文本设置为文件名，不包括扩展名。

**加载音频文件为 `MemoryStream`**：

```c#
byte[] audioBytes = System.IO.File.ReadAllBytes(newFilePath);
System.IO.MemoryStream audioStream = new System.IO.MemoryStream(audioBytes);
```

读取选择的音频文件的所有字节，并将其存入一个 `MemoryStream` 中，以便后续播放。

**更新 `buttonAudioMapping` 字典**：

```c#
if (buttonAudioMapping.ContainsKey(button))
{
    buttonAudioMapping[button].Dispose(); // 释放旧的 MemoryStream
    buttonAudioMapping[button] = audioStream;
}
else
{
    buttonAudioMapping.Add(button, audioStream);
}
```

检查字典中是否已有该按钮的映射。如果有，先释放旧的 `MemoryStream`，然后更新为新的流；如果没有，直接添加新的映射。

**更新设置中的音频文件路径**：

```c#
appSettings.ButtonAudioFilePaths[button.Name] = newFilePath;
```

**自动保存设置**：

```c#
settingsManager.SaveSettings(appSettings);
```



#### 音量大小

...略

#### 进度条

1. `Timer_Tick` 方法

```c#
private void Timer_Tick(object sender, EventArgs e)
{
    if (waveStream != null && !isTrackBarDragging)
    {
        trackBarProgress.Maximum = (int)waveStream.TotalTime.TotalSeconds;
        trackBarProgress.Value = (int)waveStream.CurrentTime.TotalSeconds;
    }
}
```

- **作用**：在定时器的每次触发时更新进度条的最大值和当前值。

- 逻辑

  ：

  - 如果 `waveStream` 不为 `null` 且当前不在拖动进度条（`isTrackBarDragging` 为 `false`），则设置进度条的最大值为音频的总时间（秒），并将当前值更新为音频当前播放时间（秒）。

2. `trackBarProgress_MouseDown` 方法

```c#
private void trackBarProgress_MouseDown(object sender, MouseEventArgs e)
{
    isTrackBarDragging = true;
}
```

- **作用**：当用户开始拖动进度条时设置标志。
- **逻辑**：将 `isTrackBarDragging` 设置为 `true`，表示用户正在拖动进度条，此时不应更新音频的播放位置。

3. `trackBarProgress_MouseUp` 方法

```c#
private void trackBarProgress_MouseUp(object sender, MouseEventArgs e)
{
    isTrackBarDragging = false;
    if (waveStream != null)
    {
        waveStream.CurrentTime = TimeSpan.FromSeconds(trackBarProgress.Value);
    }
}
```

- **作用**：当用户释放鼠标时停止拖动并更新音频播放位置。

- 逻辑

  ：

  - 将 `isTrackBarDragging` 设置为 `false`，表示拖动结束。
  - 如果 `waveStream` 不为 `null`，则根据当前进度条的值更新音频的当前播放时间。

4. `trackBarProgress_ValueChanged` 方法

```c#
private void trackBarProgress_ValueChanged(object sender, EventArgs e)
{
    if (waveStream != null && !isTrackBarDragging)
    {
        waveStream.CurrentTime = TimeSpan.FromSeconds(trackBarProgress.Value);
    }
}
```

- **作用**：当进度条的值发生变化时更新音频播放位置。

- 逻辑

  ：

  - 如果 `waveStream` 不为 `null` 且不在拖动进度条，则根据新的进度条值更新音频的当前播放时间。





## 二、快捷键

#### 实现

##### 1.右键菜单

```c#
private void Button_MouseUp(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Right)
    {
        Button button = sender as Button;
        if (button == null) return;

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        ToolStripMenuItem changeAudioFileMenuItem = new ToolStripMenuItem("更改音频文件");
        ToolStripMenuItem setShortcutKeyMenuItem = new ToolStripMenuItem("设置快捷键");

        changeAudioFileMenuItem.Click += (s, args) => ChangeAudioFile(button);
        setShortcutKeyMenuItem.Click += SetShortcutKeyMenuItem_Click;

        contextMenu.Items.Add(changeAudioFileMenuItem);
        contextMenu.Items.Add(setShortcutKeyMenuItem);
        contextMenu.Show(button, e.Location);
    }
}
```

`MouseEventArgs` 是 .NET 中用于处理鼠标事件的数据类，继承自 `EventArgs`。它提供了关于鼠标操作的信息，例如鼠标按钮的状态和鼠标指针的位置。

主要属性

- **Button**:
  - 类型: `MouseButtons`
  - 描述: 指示哪个鼠标按钮被按下或释放（如左键、右键或中键）。
- **Clicks**:
  - 类型: `int`
  - 描述: 指示在事件发生时鼠标点击的次数。
- **X**:
  - 类型: `int`
  - 描述: 鼠标指针相对于控件左上角的 X 坐标。
- **Y**:
  - 类型: `int`
  - 描述: 鼠标指针相对于控件左上角的 Y 坐标。

##### 2.快捷键设置窗口的实现

```c#
private void SetShortcutKeyMenuItem_Click(object sender, EventArgs e)
{
    ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
    if (menuItem != null)
    {
        ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
        if (owner != null)
        {
            Button button = owner.SourceControl as Button;
            if (button != null)
            {
                using (var shortcutKeyForm = new ShortcutKeyForm())
                {
                    isSettingShortcutKey = true; // 设置状态为正在设置快捷键
                    if (shortcutKeyForm.ShowDialog() == DialogResult.OK)
                    {
                        Keys selectedKey = shortcutKeyForm.SelectedKey;

                        // 检查快捷键是否已被其他按钮使用
                        if (shortcutKeyMapping.ContainsKey(selectedKey))
                        {
                            MessageBox.Show("此快捷键已被其他按钮使用，请选择其他快捷键。");
                            isSettingShortcutKey = false; // 重置状态
                            return;
                        }

                        // 如果当前按钮已经设置了快捷键，将其从映射中移除
                        var existingKey = shortcutKeyMapping.FirstOrDefault(x => x.Value == button).Key;
                        if (existingKey != Keys.None)
                        {
                            shortcutKeyMapping.Remove(existingKey);
                        }

                        // 将新的快捷键映射到按钮
                        shortcutKeyMapping[selectedKey] = button;

                        // 更新按钮对应的 Label 文本
                        if (buttonLabelMapping.TryGetValue(button, out Label label))
                        {
                            label.Text = $"快捷键: {selectedKey}";
                            label.Visible = true;
                        }
                    }
                    isSettingShortcutKey = false; // 重置状态
                }
            }
        }
    }
}
```

```c#
public partial class ShortcutKeyForm : Form
{
    public Keys SelectedKey { get; private set; }

    public ShortcutKeyForm()
    {
        InitializeComponent();
        this.KeyPreview = true; // 使 Form 可以接收按键事件
        this.Text = "快捷键绑定"; // 设置窗口标题
        // 设置窗口启动位置为屏幕中心
        this.StartPosition = FormStartPosition.CenterScreen;
    }

    private void ShortcutKeyForm_KeyDown(object sender, KeyEventArgs e)
    {
        // 检查是否是 <space> 键
        if (e.KeyCode == Keys.Space)
        {
            MessageBox.Show("不能将 <space> 键设置为快捷键。");
            e.SuppressKeyPress = true; // 防止默认行为
            return;
        }
        SelectedKey = e.KeyCode;
        lblInstruction.Text = $"按下的键: {SelectedKey}";
    }

    private void btnConfirm_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
```

##### 3.快捷键设置的响应

```c#
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    // 如果正在设置快捷键，并且尝试设置 <space> 键
    if (isSettingShortcutKey)
    {
        // 取消快捷键设置状态
        if (keyData == Keys.Space)
        {
            MessageBox.Show("不能将 <space> 键设置为快捷键。");
            isSettingShortcutKey = false; // 取消设置状态
            return true; // 表示事件已处理
        }

        // 设置其他快捷键
        shortcutKey = keyData;
        MessageBox.Show($"快捷键已设置为: {shortcutKey}");
        isSettingShortcutKey = false;

        // 检查快捷键是否已被使用
        if (shortcutKeyMapping.ContainsKey(shortcutKey))
        {
            MessageBox.Show("此快捷键已被其他按钮使用，请选择其他快捷键。");
            return true;
        }

        // 将快捷键映射到按钮上
        if (buttonLabelMapping.TryGetValue(ButtonPlay, out Label label))
        {
            // 移除旧的快捷键
            if (shortcutKeyMapping.ContainsKey(shortcutKey))
            {
                shortcutKeyMapping.Remove(shortcutKey);
            }

            label.Text = $"快捷键: {shortcutKey}";
            label.Visible = true;
            shortcutKeyMapping[shortcutKey] = ButtonPlay; // 为 ButtonPlay 设置快捷键映射
        }
        return true; // 表示事件已处理
    }

    // 处理 <space> 键，调用 ButtonPlay_Click
    if (keyData == Keys.Space)
    {
        ButtonPlay_Click(ButtonPlay, EventArgs.Empty);
        return true; // 表示事件已处理
    }

    // 检查是否启用了快捷键功能
    if (isShortcutKeyEnabled && shortcutKeyMapping.TryGetValue(keyData, out Button button))
    {
        Button_Click(button); // 触发与快捷键关联的按钮点击事件
        return true; // 表示事件已处理
    }

    // 默认处理其他键
    return base.ProcessCmdKey(ref msg, keyData);
}
```



#### 全局响应

具体来说，代码的主要功能是监控按键按下事件，并在特定的快捷键被按下时触发相应按钮的点击事件。

1. 常量和委托定义

```c#
private const int WH_KEYBOARD_LL = 13; // 钩子类型
private const int WM_KEYDOWN = 0x0100; // 按键按下消息
private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
private LowLevelKeyboardProc _proc;
private IntPtr _hookID = IntPtr.Zero;
```

- **`WH_KEYBOARD_LL`**: 定义了低级键盘钩子的类型。
- **`WM_KEYDOWN`**: 定义了键按下的消息。
- **`LowLevelKeyboardProc`**: 定义了一个委托，用于处理键盘事件的回调方法。
- **`_proc` 和 `_hookID`**: 存储钩子回调函数和钩子 ID。

2. 设置钩子

```c#
private void SetHook()
{
    _proc = HookCallback;
    _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);
}
```

- **`SetHook`**: 用于设置全局键盘钩子。
- **`SetWindowsHookEx`**: 调用 Windows API 设置钩子，传入钩子类型、回调方法和其他参数。

3. 钩子回调

```c#
private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
{
    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
    {
        Keys key = (Keys)Marshal.ReadInt32(lParam);

        // 检查快捷键映射字典
        if (shortcutKeyMapping.TryGetValue(key, out Button button))
        {
            // 触发与快捷键关联的按钮点击事件
            Button_Click(button); // 替换为你的功能调用
        }
    }
    return CallNextHookEx(_hookID, nCode, wParam, lParam);
}
```

- **`HookCallback`**: 钩子回调方法，在每次按键事件触发时被调用。

- 逻辑

  ：

  - 检查 `nCode` 是否大于等于 0（表示有效的事件）。
  - 检查 `wParam` 是否为 `WM_KEYDOWN`（表示按键按下事件）。
  - 使用 `Marshal.ReadInt32` 从 `lParam` 中读取按键值，并检查是否在 `shortcutKeyMapping` 字典中存在该键。
  - 如果存在，则触发对应按钮的点击事件。

4. 卸载钩子

```c#
private void UnhookWindowsHookEx()
{
    UnhookWindowsHookEx(_hookID);
}
```

- **`UnhookWindowsHookEx`**: 卸载全局键盘钩子，释放资源。

5. DLL 导入

```
csharp复制代码[DllImport("user32.dll")]
private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
private static extern bool UnhookWindowsHookEx(IntPtr hhk);

[DllImport("user32.dll")]
private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
```

- 这些 `DllImport` 属性导入 Windows API 函数，允许 C# 代码调用低级别的钩子功能。

6. 窗体事件

```c#
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    SetHook();
}

protected override void OnFormClosing(FormClosingEventArgs e)
{
    UnhookWindowsHookEx();
    base.OnFormClosing(e);
}
```

- **`OnLoad`**: 在窗体加载时设置钩子。
- **`OnFormClosing`**: 在窗体关闭时卸载钩子，防止资源泄露。



## 三、操作存储

```c#
private void ApplySettings()
{
    // 设置快捷键
    foreach (var kvp in appSettings.ShortcutKeys)
    {
        var buttonName = kvp.Key;
        var key = kvp.Value;

        // 查找与按钮名称对应的按钮控件
        var button = this.Controls.Find(buttonName, true).FirstOrDefault() as Button;
        if (button != null)
        {
            if (shortcutKeyMapping.ContainsKey(key))
            {
                shortcutKeyMapping.Remove(key); // 移除旧的键映射
            }
            shortcutKeyMapping[key] = button;
            if (buttonLabelMapping.TryGetValue(button, out Label label))
            {
                label.Text = $"快捷键: {key}";
                label.Visible = true;
            }
        }
    }

    // 设置音量
    volumeSlider.Volume = appSettings.Volume;

    // 加载按钮和音频文件路径映射
    foreach (var kvp in appSettings.ButtonAudioFilePaths)
    {
        var buttonName = kvp.Key;
        var filePath = kvp.Value;

        var button = this.Controls.Find(buttonName, true).FirstOrDefault() as Button;
        if (button != null && File.Exists(filePath))
        {
            byte[] audioBytes = File.ReadAllBytes(filePath);
            MemoryStream audioStream = new MemoryStream(audioBytes);
            buttonAudioMapping[button] = audioStream;

            // 更新按钮文本为文件名（不包括扩展名）
            button.Text = Path.GetFileNameWithoutExtension(filePath);
        }
    }
}

private void Form1_FormClosing(object sender, FormClosingEventArgs e)
{
    // 保存音量
    appSettings.Volume = volumeSlider.Volume;

    // 保存快捷键设置
    appSettings.ShortcutKeys.Clear();
    foreach (var kvp in shortcutKeyMapping)
    {
        var key = kvp.Key;
        var button = kvp.Value;
        appSettings.ShortcutKeys[button.Name] = key;
    }

    // 保存按钮和音频文件路径映射
    SaveButtonAudioPaths();

    // 更新 LastLoginTime
    appSettings.LastLoginTime = DateTime.Now;

    // 保存设置
    settingsManager.SaveSettings(appSettings);
}

private void SaveButtonAudioPaths()
{
    foreach (var kvp in buttonAudioMapping)
    {
        var button = kvp.Key;
        if (appSettings.ButtonAudioFilePaths.TryGetValue(button.Name, out var existingFilePath))
        {
            // 如果已经有文件路径存在，使用现有路径
            appSettings.ButtonAudioFilePaths[button.Name] = existingFilePath;
        }
    }
}


private void ShowLoginForm()
{
    DateTime lastLoginTime = DateTime.MinValue; // 初始化变量

    // 从 SettingsManager 加载设置
    AppSettings settings = settingsManager.LoadSettings();

    // 检查 LastLoginTime 是否存在和非空
    if (settings.LastLoginTime.HasValue && DateTime.Now - settings.LastLoginTime.Value < TimeSpan.FromDays(1))
    {
        return; // 如果上次登录时间在一天内，则不显示登录窗口
    }

    // 创建 LoginForm 实例并传递 SettingsManager
    LoginForm loginForm = new LoginForm(settingsManager);
    if (loginForm.ShowDialog() != DialogResult.OK)
    {
        // 如果登录不成功，关闭主窗口
        this.Close();
    }
}
```



### 1. `ApplySettings` 方法

该方法用于应用之前保存的设置，包括快捷键、音量和按钮与音频文件路径的映射。

- **快捷键设置**：
  - 遍历 `appSettings.ShortcutKeys` 字典，查找与按钮名称对应的按钮控件。
  - 如果按钮存在，移除旧的键映射，并将新映射添加到 `shortcutKeyMapping` 中。
  - 更新按钮对应的标签，显示当前快捷键。
- **音量设置**：
  - 将音量滑块的值设置为保存的音量。
- **音频文件路径加载**：
  - 遍历 `appSettings.ButtonAudioFilePaths` 字典，查找与按钮名称对应的音频文件路径。
  - 如果文件存在，则读取音频文件，存入 `buttonAudioMapping` 字典中，并更新按钮文本为文件名（不包括扩展名）。

### 2. `Form1_FormClosing` 方法

该方法在窗体关闭时调用，用于保存用户设置。

- **保存音量**：将当前音量滑块的值保存到 `appSettings.Volume`。
- **保存快捷键设置**：清空旧的快捷键设置，并遍历 `shortcutKeyMapping`，保存新的快捷键到 `appSettings.ShortcutKeys`。
- **保存按钮与音频文件路径映射**：调用 `SaveButtonAudioPaths` 方法来保存音频文件路径的映射。
- **更新最后登录时间**：将当前时间设置为 `appSettings.LastLoginTime`。
- **保存设置**：调用 `settingsManager.SaveSettings(appSettings)` 将所有设置保存到文件或数据库中。

### 3. `SaveButtonAudioPaths` 方法

这个方法用于保存按钮与音频文件路径的映射。

- 遍历 `buttonAudioMapping`，对于每个按钮，检查 `appSettings.ButtonAudioFilePaths` 中是否已经有对应的文件路径。如果有，则保留现有路径（逻辑可能需要补充具体路径更新的部分）。

### 4. `ShowLoginForm` 方法

该方法用于处理用户登录逻辑。

- **最后登录时间检查**：从 `settingsManager` 中加载设置，检查 `LastLoginTime` 是否存在且在一天内。如果是，则直接返回，不显示登录窗口。
- **显示登录窗口**：如果需要登录，创建 `LoginForm` 的实例并传递 `settingsManager`，显示登录对话框。如果用户未成功登录，则关闭主窗口。



## 四、软件的封装

略...
