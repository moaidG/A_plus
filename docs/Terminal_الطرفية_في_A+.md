# الطرفية (Terminal/Console) في لغة A+

> جميع الدوال والأوامر متوفرة بالإنجليزية والعربية.

---

## 1. الإخراج (Output)

### print / اطبع / ط
طباعة قيمة إلى المخرج (الطرفية). الأقواس اختيارية — يمكنك استخدام `print(x)` أو `print x`.

```
print(expression)
اطبع(تعبير)
ط(تعبير)
# أو بدون أقواس:
print expression
اطبع تعبير
ط تعبير
```

| الوسيط | النوع | 설명 |
|--------|------|------|
| `expression` | أي نوع | القيمة المراد طباعتها |

- **المخرج:** القيمة نفسها (للاستخدام في التعبيرات)
- **أمثلة:**
```
print("Hello World")
اطبع "مرحباً"           # بدون أقواس
ط(10 + 20)

let x = print "قيمة x هي: " + x   # يمكن استخدامها في تعبير
```

### اختصارات الطباعة السريعة (Shorthand)

| الاختصار | يتحول إلى |
|---------|----------|
| `print>"text"` | `print("text")` |
| `اطبع>"نص"` | `print("نص")` |
| `ط>"نص"` | `print("نص")` |
| `ط>expr` | `print(expr)` |

---

## 2. الإدخال (Input)

### input / اقرأ
قراءة سطر نصي من المستخدم.

```
input(prompt)
اقرأ(مطلب)
```

| الوسيط | النوع | افتراضي | الوصف |
|--------|------|---------|-------|
| `prompt` | نص | `""` | نص توجيهي يظهر قبل الإدخال |

- **المخرج:** نص (ما أدخله المستخدم)
- **أمثلة:**
```
let name = input("ادخل اسمك: ")
print("مرحباً " + name)

let age = input("كم عمرك؟ ")
let ageNum = age + 0   # تحويل النص إلى رقم
```

---

## 3. التحكم في الطرفية (Console Control)

### consoleClear / مسحشاشة
مسح شاشة الطرفية بالكامل.

```
consoleClear()
مسحشاشة()
```

- **المُدخل:** لا شيء
- **المخرج:** 0
- **مثال:**
```
consoleClear()
print("الشاشة نظيفة!")
```

### consoleSetCursor / حددموقعمؤشر
تحديد موقع المؤشر في الطرفية.

```
consoleSetCursor(x, y)
حددموقعمؤشر(س, ص)
```

| الوسيط | النوع | افتراضي | الوصف |
|--------|------|---------|-------|
| `x` | رقم | 0 | العمود (0-based) |
| `y` | رقم | 0 | الصف (0-based) |

- **المخرج:** 0
- **مثال:**
```
consoleClear()
consoleSetCursor(10, 5)
print("مرحباً")      # يظهر في العمود 10، الصف 5
```

### consoleColor / لوننص
تغيير لون النص في الطرفية.

```
consoleColor(colorName)
لوننص(اسم_اللون)
```

| الوسيط | النوع | افتراضي | الوصف |
|--------|------|---------|-------|
| `colorName` | نص | `"white"` | اسم اللون (غير حساس لحالة الأحرف) |

- **المخرج:** 0

**الألوان المدعومة (16 لوناً):**

| الإنجليزي | العربية | المعاينة |
|-----------|---------|---------|
| `black` | أسود | ⬛ |
| `blue` | أزرق | 🟦 |
| `cyan` | سماوي | 🌐 |
| `darkblue` | أزرق غامق | |
| `darkcyan` | سماوي غامق | |
| `darkgray` | رمادي غامق | |
| `darkgreen` | أخضر غامق | |
| `darkmagenta` | أرجواني غامق | |
| `darkred` | أحمر غامق | |
| `darkyellow` | أصفر غامق | |
| `gray` | رمادي | |
| `green` | أخضر | 🟩 |
| `magenta` | أرجواني | |
| `red` | أحمر | 🟥 |
| `white` | أبيض | ⬜ |
| `yellow` | أصفر | 🟨 |

- **أمثلة:**
```
consoleColor("red")
print("هذا النص أحمر")
consoleColor("green")
print("هذا النص أخضر")
consoleColor("white")
print("عودة إلى الأبيض")

لوننص("أزرق")
اطبع("هذا النص أزرق")
```

---

## 4. المنصة (Platform)

### platform / منصة
معرفة نظام التشغيل الحالي.

```
platform()
منصة()
```

- **المُدخل:** لا شيء
- **المخرج:** نص — أحد القيم: `"windows"`, `"linux"`, `"macos"`, `"other"`
- **مثال:**
```
if (platform() == "windows") {
    print("أنت على ويندوز")
} else if () {
    print("أنت على لينكس")
} else {
    print("نظام آخر: " + platform())
}
```

---

## 5. التصحيح (Debug)

### trace / تتبع
تسجيل رسالة تتبع لأغراض التصحيح.

```
trace(message)
تتبع(الرسالة)
```

| الوسيط | النوع | الوصف |
|--------|------|-------|
| `message` | نص | الرسالة المراد تسجيلها |

- **المخرج:** 0
- **مثال:**
```
func calculate(x) {
    trace("دخلنا الدالة calculate بقيمة: " + x)
    return x * 2
}
```

### debugger / مصحح
إيقاف التنفيذ مؤقتاً وعرض المتغيرات المحلية.

```
debugger
مصحح
```

- **مثال:**
```
let a = 10
let b = 20
debugger
print(a + b)
```
- **المخرج:**
```
[مصحح] سطر 4, عمود 1 | المتغيرات: a=10, b=20
```

عند استخدام العلم `-d` (وضع DAP)، يتوقف التنفيذ ويمكن تتبعه من VS Code.

---

## 6. وضع التفاعل (REPL)

تشغيل وضع الأوامر التفاعلية من الطرفية:

```
a+ -i
a+ --interactive
a+ -repl
```

```
A+ Interactive REPL (أ+ وضع تفاعلي)
اكتب 'exit' أو 'خروج' للخروج
> let x = 10
> print(x * 2)
20
> x + 5
=> 15
> "Hello" + " " + "World"
=> "Hello World"
> خروج
```

- كل سطر يُفسر فوراً ويظهر النتيجة بـ `=>`
- الأوامر تدعم التعبيرات، المتغيرات، الحلقات، والدوال
- كتابة `exit` أو `خروج` للخروج

---

## 7. واجهة المستخدم النصية (Console TUI)

عند تشغيل A+ على لينكس أو ماك (حيث لا يتوفر WPF)، تستخدم `ConsoleRuntime` لعرض واجهة المستخدم كنص في الطرفية.

### كيفية العرض

```
a+ new stackpanel panel
a+ new textblock label
label.text = "Hello from Console UI!"
a+ new button btn
btn.content = "Click Me"
panel.add(label)
panel.add(btn)
show()
```

**الخرج في الطرفية:**
```
═══════════════════════════════════
 A+ Console UI (وضع النص)
═══════════════════════════════════
[stackpanel]
  [textblock]: Hello from Console UI!
  [button]: Click Me
═══════════════════════════════════
اضغط أي مفتاح للخروج...
```

### العناصر المدعومة في Console TUI

جميع أنواع عناصر A+ مدعومة — تُعرض بشكل شجري مع مسافة بادئة (indent):

```
[navafxda]              # تخطيط
  [نص]: مرحباً          # نصوص
  [زر]: تشغيل            # أزرار
  [مكدسة]               # لوحات متداخلة
    [صندوق_نص]: اكتب... # إدخال
```

### MediaElement في الطرفية

```
mediaelement new player
player.Source = "intro.mp4"
player.play()       # → [وسائط] تشغيل: intro.mp4
player.pause()      # → [وسائط] تم الإيقاف المؤقت
player.stop()       # → [وسائط] تم الإيقاف
player.شغل()        # بالعربية أيضاً
player.أوقف()
player.إيقاف()
```

### الأحداث في Console TUI

الأحداث تُسجل في `ConsoleElement.EventHandlers` لكن لا تُطلق تلقائياً (لا يوجد تفاعل في وضع النص):

```
btn.Click = fn() { print("تم النقر!") }
```

---

## 8. المكتبة المساعدة lib_console.a

مكتبة A+ إضافية للاستخدام في الطرفية. تستورد بـ `include`:

```
include "lib_console.a"
```

### printLine
طباعة سطر.

```
printLine(msg)
```

### printList
طباعة عناصر مصفوفة كل في سطر.

```
printList(items)
```

### printRepeat
طباعة رسالة عدة مرات.

```
printRepeat(msg, times)
```

### readNumber
قراءة رقم من المستخدم (تحويل النص المدخل إلى رقم تلقائياً).

```
readNumber(prompt)
```

### confirm
سؤال المستخدم بنعم/لا.

```
confirm(prompt)     # يرجع 1 (نعم) أو 0 (لا)
```

### wait
انتظار عدد من الثواني (باستخدام `dotnet`).

```
wait(seconds)
```

### count
عد عناصر مصفوفة.

```
count(items)
```

### مثال كامل:

```
include "lib_console.a"

printLine("برنامج الطرفية!")
let name = input("ادخل اسمك: ")
printLine("مرحباً " + name)

let data = [1, 2, 3, 4, 5]
printLine("المصفوفة:")
printList(data)

if confirm("هل تريد الاستمرار؟") {
    printLine("نكمل!")
} else {
    printLine("إلى اللقاء")
}
```

---

## 9. أمثلة شاملة

### مثال 1: قائمة ألوان

```
consoleClear()
consoleColor("darkblue")
print("===== ألوان A+ =====")

let colors = ["red", "green", "yellow", "blue", "magenta", "cyan"]
for c in colors {
    consoleColor(c)
    print("هذا اللون: " + c)
}

consoleColor("white")
print("انتهى العرض")
```

### مثال 2: لعبة تخمين الأرقام

```
let target = 42
let guess = 0
let attempts = 0

consoleClear()
consoleColor("cyan")
print("===== لعبة تخمين الرقم =====")
consoleColor("white")

while guess != target {
    let inputStr = input("خمن رقماً: ")
    guess = inputStr + 0
    attempts = attempts + 1

    if guess < target {
        consoleColor("red")
        print("أقل من الرقم!")
    } else if guess > target {
        consoleColor("red")
        print("أكبر من الرقم!")
    } else {
        consoleColor("green")
        print("أحسنت! الرقم هو " + target)
        consoleColor("yellow")
        print("عدد المحاولات: " + attempts)
    }
    consoleColor("white")
}
```

### مثال 3: شريط تقدم نصي

```
func showProgress(current, total) {
    let percent = (current / total) * 100
    let bars = percent / 5
    let bar = ""
    let i = 0
    while i < bars {
        bar = bar + "█"
        i = i + 1
    }
    while i < 20 {
        bar = bar + "░"
        i = i + 1
    }
    consoleSetCursor(0, 0)
    consoleColor("green")
    print("[" + bar + "] " + percent + "%")
}

consoleClear()
let i = 0
while i <= 100 {
    showProgress(i, 100)
    i = i + 1
}
consoleColor("white")
print("\nتم!")
```

### مثال 4: Console TUI بالعربية

```
<مكدسة العرض="300" الارتفاع="200">
    <نص النص="Console UI بالعربية" حجم_الخط="20" />
    <نص النص="هذا تطبيق طرفية" />
    <زر المحتوى="تشغيل" />
    <صندوق_نص النص="اكتب هنا..." />
</مكدسة>
show()
```

**الخرج:**
```
═══════════════════════════════════
 A+ Console UI (وضع النص)
═══════════════════════════════════
[stackpanel]
  [textblock]: Console UI بالعربية
  [textblock]: هذا تطبيق طرفية
  [button]: تشغيل
  [textbox]: اكتب هنا...
═══════════════════════════════════
اضغط أي مفتاح للخروج...
```

---

## 10. ملخص الدوال

| الإنجليزي | العربي | الوصف |
|-----------|--------|-------|
| `print(x)` / `print x` | `اطبع(x)` / `اطبع x` / `ط(x)` / `ط x` | طباعة قيمة (الأقواس اختيارية) |
| `input(p)` | `اقرأ(م)` | قراءة إدخال من المستخدم |
| `consoleClear()` | `مسحشاشة()` | مسح الشاشة |
| `consoleSetCursor(x,y)` | `حددموقعمؤشر(س,ص)` | تحريك المؤشر |
| `consoleColor(name)` | `لوننص(اسم)` | تغيير لون النص (16 لوناً) |
| `platform()` | `منصة()` | معرفة نظام التشغيل |
| `trace(msg)` | `تتبع(رسالة)` | تسجيل تتبع |
| `debugger` | `مصحح` | إيقاف للتصحيح |
| `typeof(x)` | `نوع(س)` | معرفة نوع القيمة |
| `help(name)` | `مساعدة(اسم)` | عرض مساعدة عن دالة |
