# Type System & Generics (الأنواع والمولدات)

يدعم A+ نظام أنواع بسيط مع مولدات (generics).

## Type Annotations (تعليقات الأنواع)

```
let x: number = 10
let name: string = "A+"
let flag: bool = true
```

يمكن تحديد نوع المتغير بعد `:`.

## Function Type Annotations (أنواع الدوال)

```
func add(a: number, b: number): number {
    return a + b
}
```

يمكن تحديد أنواع المعاملات ونوع القيمة المُرجَعة.

## Generics (المولدات)

```
func first[T](list: [T]): T {
    return list[0]
}

class Box[T] {
    let value: T
}
```

المولدات تُستخدم مع قوسين مربعين `[T]`. حالياً يتم محو النوع (type erasure) — يُعامل `T` كـ `object` أثناء التنفيذ.

## الأنواع المدعومة

- `number` — رقم (double)
- `string` — نص
- `bool` — قيمة منطقية
- `nil` — قيمة فارغة
- `[T]` — مصفوفة من النوع T
- `dict` — قاموس
- `func` — دالة

ملاحظة: التحقق من الأنواع في وقت التنفيذ حالياً محدود (قيد التطوير).
