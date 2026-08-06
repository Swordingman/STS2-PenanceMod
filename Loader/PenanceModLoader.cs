using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Modding;

namespace PenanceMod.Loader;

[ModInitializer("Init")]
public static class PenanceModLoader
{
    private const string ModId = "PenanceMod";

    private const string MainFileTypeName =
        "PenanceMod.PenanceModCode.MainFile";

    private static bool _initialized;
    private static string _loaderDirectory = string.Empty;

    private static readonly Assembly LoaderAssembly =
        typeof(PenanceModLoader).Assembly;

    private static readonly AssemblyLoadContext ModLoadContext =
        AssemblyLoadContext.GetLoadContext(LoaderAssembly)
        ?? AssemblyLoadContext.Default;

    public static void Init()
    {
        if (_initialized)
        {
            Console.WriteLine(
                "[PenanceMod.Loader] 已初始化，跳过重复调用。");

            return;
        }

        try
        {
            _loaderDirectory =
                Path.GetDirectoryName(LoaderAssembly.Location)
                ?? throw new InvalidOperationException(
                    "无法取得 PenanceMod Loader 所在目录。");

            /*
             * 必须在加载实现 DLL 前注册依赖解析器。
             * 否则 PenanceMod.Stable.dll 或 PenanceMod.Beta.dll
             * 读取 BaseLib 类型时可能找不到 BaseLib.dll。
             */
            ModLoadContext.Resolving -= ResolveDependency;
            ModLoadContext.Resolving += ResolveDependency;

            ApiBranch branch = DetectApiBranch();

            string implementationName = branch switch
            {
                ApiBranch.Stable => "PenanceMod.Stable.dll",
                ApiBranch.Beta => "PenanceMod.Beta.dll",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(branch),
                    branch,
                    "未知的游戏 API 分支。")
            };

            string implementationPath =
                Path.Combine(
                    _loaderDirectory,
                    implementationName);

            if (!File.Exists(implementationPath))
            {
                throw new FileNotFoundException(
                    $"找不到实现 DLL：{implementationPath}",
                    implementationPath);
            }

            Console.WriteLine(
                $"[PenanceMod.Loader] 检测到 {branch} API，"
                + $"加载 {implementationName}");

            Assembly implementation =
                LoadImplementation(implementationPath);

            HandleAssemblyAssociation(
                branch,
                implementation);

            InvokeMainFileInit(implementation);

            _initialized = true;

            Console.WriteLine(
                $"[PenanceMod.Loader] "
                + $"{implementationName} 初始化成功。");

            Console.WriteLine(
                $"[PenanceMod.Loader] Loader Assembly："
                + $"{LoaderAssembly.GetName().Name}");

            Console.WriteLine(
                $"[PenanceMod.Loader] Implementation Assembly："
                + $"{implementation.GetName().Name}");

            Console.WriteLine(
                $"[PenanceMod.Loader] Loader ALC："
                + $"{GetLoadContextName(LoaderAssembly)}");

            Console.WriteLine(
                $"[PenanceMod.Loader] Implementation ALC："
                + $"{GetLoadContextName(implementation)}");

            AssemblyLoadContext? loaderContext =
                AssemblyLoadContext.GetLoadContext(LoaderAssembly);

            AssemblyLoadContext? implementationContext =
                AssemblyLoadContext.GetLoadContext(implementation);

            if (!ReferenceEquals(
                    loaderContext,
                    implementationContext))
            {
                Console.Error.WriteLine(
                    "[PenanceMod.Loader] 警告：Loader 与实现程序集"
                    + "不在同一个 AssemblyLoadContext 中。");
            }
        }
        catch (ReflectionTypeLoadException exception)
        {
            _initialized = false;

            Console.Error.WriteLine(
                "[PenanceMod.Loader] 实现程序集类型加载失败：");

            foreach (Exception? loaderException
                     in exception.LoaderExceptions)
            {
                if (loaderException == null)
                    continue;

                Console.Error.WriteLine(
                    $"[PenanceMod.Loader] LoaderException："
                    + $"{loaderException}");
            }

            throw;
        }
        catch (Exception exception)
        {
            _initialized = false;

            Console.Error.WriteLine(
                $"[PenanceMod.Loader] 加载失败：{exception}");

            throw;
        }
    }

    private static Assembly LoadImplementation(
        string implementationPath)
    {
        string fullPath =
            Path.GetFullPath(implementationPath);

        Assembly? existing =
            ModLoadContext.Assemblies.FirstOrDefault(
                assembly =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(
                                assembly.Location))
                        {
                            return false;
                        }

                        string loadedPath =
                            Path.GetFullPath(
                                assembly.Location);

                        return string.Equals(
                            loadedPath,
                            fullPath,
                            StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                });

        if (existing != null)
        {
            Console.WriteLine(
                "[PenanceMod.Loader] 实现程序集已经加载，复用："
                + $"{existing.GetName().Name}");

            return existing;
        }

        Console.WriteLine(
            $"[PenanceMod.Loader] 从路径加载实现程序集："
            + $"{fullPath}");

        return ModLoadContext.LoadFromAssemblyPath(fullPath);
    }

    private static void HandleAssemblyAssociation(
        ApiBranch branch,
        Assembly implementation)
    {
        if (branch == ApiBranch.Stable)
        {
            /*
             * Stable ModManager 仅记录一个主程序集。
             * PenanceMod.Stable.dll 会在 MainFile.Init() 中
             * 自行安装 ReflectionHelper.ModTypes 类型发现桥。
             *
             * Loader 不使用 Harmony，也不修改 Mod.assembly。
             */
            Console.WriteLine(
                "[PenanceMod.Loader] Stable 实现将在 "
                + "MainFile.Init() 中安装类型发现桥，"
                + "Loader 跳过程序集关联。");

            return;
        }

        if (TryAssociateWithCurrentMod(implementation))
        {
            Console.WriteLine(
                $"[PenanceMod.Loader] 已将 "
                + $"{implementation.GetName().Name} "
                + $"关联到 Mod {ModId}。");

            return;
        }

        Console.Error.WriteLine(
            "[PenanceMod.Loader] 警告：未能将 Beta 实现程序集"
            + $"关联到 {ModId}，将继续初始化。");
    }

    private static Assembly? ResolveDependency(
        AssemblyLoadContext context,
        AssemblyName requestedAssembly)
    {
        string? requestedName =
            requestedAssembly.Name;

        if (string.IsNullOrWhiteSpace(requestedName))
            return null;

        Assembly? alreadyLoaded =
            context.Assemblies.FirstOrDefault(
                assembly =>
                    string.Equals(
                        assembly.GetName().Name,
                        requestedName,
                        StringComparison.OrdinalIgnoreCase));

        if (alreadyLoaded != null)
            return alreadyLoaded;

        try
        {
            string modsDirectory =
                Directory.GetParent(_loaderDirectory)?.FullName
                ?? throw new DirectoryNotFoundException(
                    "无法取得游戏 mods 目录。");

            string gameDirectory =
                Directory.GetParent(modsDirectory)?.FullName
                ?? throw new DirectoryNotFoundException(
                    "无法取得游戏根目录。");

            string[] candidatePaths =
            [
                /*
                 * PenanceMod 自己的目录。
                 * 用于寻找实现 DLL 旁边的其他依赖。
                 */
                Path.Combine(
                    _loaderDirectory,
                    $"{requestedName}.dll"),

                /*
                 * 其他模组目录。
                 * 例如：
                 * mods\BaseLib\BaseLib.dll
                 */
                Path.Combine(
                    modsDirectory,
                    requestedName,
                    $"{requestedName}.dll"),

                /*
                 * 游戏原生程序集目录。
                 */
                Path.Combine(
                    gameDirectory,
                    "data_sts2_windows_x86_64",
                    $"{requestedName}.dll")
            ];

            foreach (string candidatePath
                     in candidatePaths.Distinct(
                         StringComparer.OrdinalIgnoreCase))
            {
                if (!File.Exists(candidatePath))
                    continue;

                string fullPath =
                    Path.GetFullPath(candidatePath);

                Assembly? loadedDuringSearch =
                    context.Assemblies.FirstOrDefault(
                        assembly =>
                            string.Equals(
                                assembly.GetName().Name,
                                requestedName,
                                StringComparison.OrdinalIgnoreCase));

                if (loadedDuringSearch != null)
                    return loadedDuringSearch;

                Console.WriteLine(
                    $"[PenanceMod.Loader] 加载依赖：{fullPath}");

                return context.LoadFromAssemblyPath(fullPath);
            }

            Console.Error.WriteLine(
                $"[PenanceMod.Loader] 找不到依赖："
                + $"{requestedAssembly.FullName}");

            return null;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"[PenanceMod.Loader] 解析依赖 "
                + $"{requestedAssembly.FullName} 时失败："
                + $"{exception}");

            return null;
        }
    }

    private static ApiBranch DetectApiBranch()
    {
        Assembly sts2Assembly =
            GetSts2Assembly();

        const string attackCommandTypeName =
            "MegaCrit.Sts2.Core.Commands.Builders.AttackCommand";

        Type attackCommandType =
            sts2Assembly.GetType(
                attackCommandTypeName,
                throwOnError: true)
            ?? throw new TypeLoadException(
                $"找不到类型：{attackCommandTypeName}");

        MethodInfo[] fromCardMethods =
            attackCommandType
                .GetMethods(
                    BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic)
                .Where(method =>
                    method.Name == "FromCard")
                .ToArray();

        bool stableApi =
            fromCardMethods.Any(
                method =>
                {
                    ParameterInfo[] parameters =
                        method.GetParameters();

                    return parameters.Length == 1
                        && parameters[0].ParameterType.Name
                        == "CardModel";
                });

        bool betaApi =
            fromCardMethods.Any(
                method =>
                {
                    ParameterInfo[] parameters =
                        method.GetParameters();

                    return parameters.Length == 2
                        && parameters[0].ParameterType.Name
                        == "CardModel"
                        && parameters[1].ParameterType.Name
                        == "CardPlay";
                });

        Console.WriteLine(
            $"[PenanceMod.Loader] API 探测结果："
            + $"Stable={stableApi}，Beta={betaApi}");

        if (stableApi && !betaApi)
            return ApiBranch.Stable;

        if (betaApi && !stableApi)
            return ApiBranch.Beta;

        throw new NotSupportedException(
            $"无法判断当前游戏 API："
            + $"Stable={stableApi}，Beta={betaApi}。");
    }

    private static void InvokeMainFileInit(
        Assembly implementation)
    {
        Type mainFileType =
            implementation.GetType(
                MainFileTypeName,
                throwOnError: true)
            ?? throw new TypeLoadException(
                $"找不到实现入口类型："
                + $"{MainFileTypeName}");

        MethodInfo initMethod =
            mainFileType.GetMethod(
                "Init",
                BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null)
            ?? throw new MissingMethodException(
                MainFileTypeName,
                "Init");

        Console.WriteLine(
            $"[PenanceMod.Loader] 调用实现入口："
            + $"{MainFileTypeName}.Init()");

        try
        {
            initMethod.Invoke(null, null);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    private static Assembly GetSts2Assembly()
    {
        Assembly? assembly =
            AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(
                    candidate =>
                        string.Equals(
                            candidate.GetName().Name,
                            "sts2",
                            StringComparison.OrdinalIgnoreCase));

        return assembly
            ?? throw new InvalidOperationException(
                "没有找到已经加载的 sts2.dll。");
    }

    private static bool TryAssociateWithCurrentMod(
        Assembly implementation)
    {
        try
        {
            MethodInfo? associateMethod =
                typeof(ModManager).GetMethod(
                    "AssociateAssemblyWithMod",
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Static,
                    binder: null,
                    types:
                    [
                        typeof(string),
                        typeof(Assembly)
                    ],
                    modifiers: null);

            if (associateMethod == null)
            {
                Console.Error.WriteLine(
                    "[PenanceMod.Loader] 当前 Beta API 不存在 "
                    + "ModManager.AssociateAssemblyWithMod"
                    + "(string, Assembly)。");

                return false;
            }

            associateMethod.Invoke(
                null,
                [
                    ModId,
                    implementation
                ]);

            return true;
        }
        catch (TargetInvocationException exception)
        {
            Exception actualException =
                exception.InnerException ?? exception;

            Console.Error.WriteLine(
                "[PenanceMod.Loader] AssociateAssemblyWithMod "
                + $"调用失败：{actualException}");

            return false;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                "[PenanceMod.Loader] 实现程序集关联失败："
                + $"{exception}");

            return false;
        }
    }

    private static string GetLoadContextName(
        Assembly assembly)
    {
        AssemblyLoadContext? context =
            AssemblyLoadContext.GetLoadContext(assembly);

        if (context == null)
            return "<null>";

        return context.Name
            ?? context.ToString()
            ?? "<未命名>";
    }

    private enum ApiBranch
    {
        Stable,
        Beta
    }
}