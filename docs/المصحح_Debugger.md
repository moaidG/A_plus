# Debugger (المصحح)

يدعم A+ التصحيح عبر كلمة `debugger` ووضع DAP (Debug Adapter Protocol).

## Debugger Statement (جملة المصحح)

```
let x = 10
debugger          // يتوقف هنا ويطبع المتغيرات المحلية
print(x)
```

عندما يصل التنفيذ إلى `debugger`:
- يطبع رقم السطر والعمود الحاليين
- يطبع جميع المتغيرات المحلية مع قيمها

## Trace (تتبع)

```
trace("بدأت العملية")
let result = doWork()
trace("النتيجة: " + result)
```

`trace()` تسجل رسالة تتبع — مفيدة لفهم تدفق التنفيذ.

## وضع التصحيح المتقدم (DAP Mode)

```
a+ -d <file.a>     // تشغيل الملف في وضع DAP
```

هذا الوضع يتواصل عبر بروتوكول DAP (Debug Adapter Protocol) على stdin/stdout،
للتكامل مع VS Code ومحررات أخرى تدعم DAP.
