#if !STS2_BETA

using System.Reflection;
using MegaCrit.Sts2.Core.Modding;

namespace PenanceMod.PenanceModCode;

/// <summary>
/// 正式版 ModManager 只允许一个程序集作为 Mod 的登记程序集。
///
/// Loader 初始化期间，ModManager 尚未给当前 Mod 的 assembly 字段赋值。
/// 因此这里等待 OnModDetected，在游戏完成赋值后，把 Loader 程序集替换
/// 为真正包含角色、卡牌、遗物等模型的 Stable 实现程序集。
/// </summary>
internal static class StableAssemblyRegistration
{
    private const string ModId = "PenanceMod";

    private static bool _installed;
    private static bool _completed;

    private static Assembly? _implementationAssembly;

    public static void Install(Assembly implementationAssembly)
    {
        ArgumentNullException.ThrowIfNull(implementationAssembly);

        if (_completed)
        {
            Console.WriteLine(
                "[PenanceMod] Stable 实现程序集已经登记完成。");

            return;
        }

        _implementationAssembly = implementationAssembly;

        if (_installed)
        {
            Console.WriteLine(
                "[PenanceMod] Stable 程序集登记处理器已经安装。");

            return;
        }

        ModManager.OnModDetected += OnModDetected;
        _installed = true;

        Console.WriteLine(
            "[PenanceMod] 已安装 Stable 程序集延迟登记处理器。");
    }

    private static void OnModDetected(Mod mod)
    {
        try
        {
            string? detectedModId = mod.manifest?.id;

            if (!string.Equals(
                    detectedModId,
                    ModId,
                    StringComparison.Ordinal))
            {
                return;
            }

            Assembly implementationAssembly =
                _implementationAssembly
                ?? throw new InvalidOperationException(
                    "Stable 实现程序集尚未保存。");

            Assembly? previousAssembly = mod.assembly;

            if (ReferenceEquals(
                    previousAssembly,
                    implementationAssembly))
            {
                Console.WriteLine(
                    "[PenanceMod] Stable 实现程序集已经是当前 Mod "
                    + "的登记程序集。");

                Complete();
                return;
            }

            /*
             * 此时 ModManager 已完成：
             *
             * mod.state = Loaded
             * mod.assembly = PenanceMod.Loader.dll
             *
             * 将其换成真正包含所有游戏模型的 Stable 实现程序集。
             */
            mod.assembly = implementationAssembly;

            Console.WriteLine(
                "[PenanceMod] Stable Mod 登记程序集已切换："
                + $"{previousAssembly?.GetName().Name ?? "<null>"}"
                + " -> "
                + $"{implementationAssembly.GetName().Name}");

            VerifyRegistration(mod, implementationAssembly);

            Complete();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"[PenanceMod] Stable 程序集登记失败：{exception}");

            /*
             * 失败后仍然取消事件，避免后续事件不断重复抛异常。
             * 启动过程随后会通过缺少模型或网络 ID 的错误暴露问题。
             */
            Complete();
        }
    }

    private static void VerifyRegistration(
        Mod mod,
        Assembly expectedAssembly)
    {
        if (!ReferenceEquals(
                mod.assembly,
                expectedAssembly))
        {
            throw new InvalidOperationException(
                "写入 Mod.assembly 后验证失败。");
        }

        Type[] modelTypes;

        try
        {
            modelTypes = expectedAssembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract
                    && !type.IsInterface
                    && IsAbstractModel(type))
                .ToArray();
        }
        catch (ReflectionTypeLoadException exception)
        {
            foreach (Exception? loaderException
                     in exception.LoaderExceptions)
            {
                if (loaderException != null)
                {
                    Console.Error.WriteLine(
                        "[PenanceMod] 类型加载异常："
                        + $"{loaderException}");
                }
            }

            modelTypes = exception.Types
                .OfType<Type>()
                .Where(type =>
                    !type.IsAbstract
                    && !type.IsInterface
                    && IsAbstractModel(type))
                .ToArray();
        }

        Console.WriteLine(
            $"[PenanceMod] Stable 登记程序集中的模型类型数："
            + $"{modelTypes.Length}");

        Type? medalType =
            modelTypes.FirstOrDefault(type =>
                type.Name.Contains(
                    "MedalOfPerseverance",
                    StringComparison.OrdinalIgnoreCase)
                || type.Name.Contains(
                    "MEDAL_OF_PERSEVERANCE",
                    StringComparison.OrdinalIgnoreCase));

        if (medalType != null)
        {
            Console.WriteLine(
                "[PenanceMod] 已在登记程序集中发现目标模型："
                + $"{medalType.FullName}");
        }
    }

    private static bool IsAbstractModel(Type type)
    {
        Type? current = type;

        while (current != null)
        {
            if (string.Equals(
                    current.FullName,
                    "MegaCrit.Sts2.Core.Models.AbstractModel",
                    StringComparison.Ordinal))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static void Complete()
    {
        if (_installed)
        {
            ModManager.OnModDetected -= OnModDetected;
        }

        _installed = false;
        _completed = true;
    }
}

#endif