# Regular Expressions (التعبيرات المنتظمة)

يدعم A+ التعبيرات المنتظمة (regex) المبنية على `System.Text.RegularExpressions.Regex`.

## .test() — اختبار التطابق

```
let pattern = /hello/
let result = pattern.test("hello world")
print(result)  // true
```

`/pattern/` يُنشئ تعبيراً منتظماً (Regex كائن).
`.test(string)` يرجع `true` إذا وجد تطابقاً.

## .match() — إيجاد التطابقات

```
let pattern = /\d+/
let result = pattern.match("abc123def456")
print(result)  // ["123", "456"]
```

`.match(string)` يرجع مصفوفة من جميع التطابقات.

## .replace() — استبدال التطابقات

```
let pattern = /world/
let result = pattern.replace("hello world", "A+")
print(result)  // "hello A+"
```

`.replace(input, replacement)` يستبدل جميع التطابقات بالنص البديل.

## ملاحظات

- التعبير المنتظم يُكتب بين `/` كما في JavaScript
- يتم التحويل إلى `System.Text.RegularExpressions.Regex` خلف الكواليس
- لا حاجة لاستيراد أي مكتبة — هذه دوال مدمجة
