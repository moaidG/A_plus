# Module System (نظام الوحدات)

يتيح لك A+ تنظيم الكود في ملفات ووحدات قابلة لإعادة الاستخدام.

## Export (تصدير)

```
// file: math.a
export let PI = 3.14159
export func double(x) { return x * 2 }
export class Calculator {
    func add(a, b) { return a + b }
}
```

## Import (استيراد)

```
// استيراد أسماء محددة
import { PI, double, Calculator } from "math"

print(PI)
print(double(5))

let calc = new Calculator
print(calc.add(3, 4))
```

## Import as Alias (استيراد كاسم مستعار)

```
import "math" as m
print(m.PI)
print(m.double(5))
```

الاستيراد كاسم مستعار يخزّن كل التصديرات في `dict` واحد.

## مسار البحث

يتم البحث عن الملفات المستوردة نسبة إلى:
1. المجلد الحالي (حيث يوجد الملف الرئيسي)
2. مسار `StdLibPath` (مجلد المكتبة القياسية)

## الكشف عن الاستيراد الدائري

يتم اكتشاف الاستيراد الدائري (circular import) تلقائياً مع رسالة خطأ واضحة.
