# Pipe Operator | (عامل الأنبوب)

يدعم A+ عامل الأنبوب `|>` لربط الدوال من اليسار إلى اليمين.

## الاستخدام الأساسي

```
let result = 5 | double | add(3)
// يعادل: add(double(5), 3)
```

قيمة التعبير على اليسار تُمرّر كأول وسيط للدالة على اليمين.

## مع دوال متعددة الوسائط

```
func add(a, b) { return a + b }
func multiply(a, b) { return a * b }

let result = 10 | multiply(2) | add(5)
// يعادل: add(multiply(10, 2), 5) = 25
```

## الأولوية (Precedence)

عامل الأنبوب له أولوية أقل من `or`/`و` و `and`/`أو`:

```
let x = 5 | double + 3  // double(5) + 3 = 13
```

## أمثلة إضافية

```
// تحويل سلسلة
let upper = "hello" | toUpper | trim

// سلسلة عمليات رياضية
let calc = 100 | divide(4) | sqrt | round
```
