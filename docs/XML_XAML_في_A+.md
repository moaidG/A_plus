# XML/XAML في لغة A+

لغة A+ تدعم كتابة واجهات المستخدم (UI) بثلاث طرق: أكواد A+ العادية، أو XML مضمن في الكود، أو XAML كامل inline بين `<` و `>`.

> جميع الوسوم والخصائص متوفرة بالعربية والإنجليزية.

---

## 1. الطرق الثلاث لإنشاء واجهة المستخدم

### الطريقة الأولى: `a+ new` — أوامر عادية

```
a+ new stackpanel panel
a+ new textblock label
label.text = "مرحباً"
a+ new button btn
btn.content = "انقرني"
panel.add(label)
panel.add(btn)
show()
```

### الطريقة الثانية: XML مضمن (Inline) بعد `a+ new`

```
a+ new button btn = <Button Content="OK" Width="100" />

# أو بالعربية:
a+ new زر زر1 = <زر المحتوى="تشغيل" العرض="100" />
```

### الطريقة الثالثة: كتل XML حرة (Free-form)

أي كود XML خارج `a+ new` يتحول تلقائياً إلى أوامر `a+ new`:

```
<StackPanel Width="400" Height="300">
    <TextBlock Text="A+ XML UI" FontSize="24" Foreground="Blue" />
    <Button Content="OK" Width="100" Click="handleClick()" />
</StackPanel>
show()
```

---

## 2. أنواع عناصر واجهة المستخدم

### تخطيط (Layout) — بالعربية والإنجليزية

| XML (إنجليزي) | XML (عربي) |説明 |
|--------------|-----------|------|
| `<Grid>` | `<شبكة>` | شبكة |
| `<StackPanel>` | `<لوحة_مكدسة>` أو `<مكدسة>` | لوحة مكدسة (عمودي/أفقي) |
| `<WrapPanel>` | `<لوحة_ملتفة>` أو `<ملتفة>` | لوحة التفاف |
| `<DockPanel>` | `<لوحة_إرساء>` أو `<إرساء>` | لوحة إرساء |
| `<Canvas>` | `<لوحة_رسم>` أو `<رسم>` | لوحة رسم |
| `<UniformGrid>` | `<شبكة_موحدة>` | شبكة موحدة |
| `<Viewbox>` | `<صندوق_عرض>` | صندوق عرض |
| `<Border>` | `<حدود>` | حدود |
| `<ScrollViewer>` | `<متصفح_تمرير>` أو `<تمرير>` | متصفح تمرير |

### نصوص (Text)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<TextBlock>` | `<نص>` | نص ثابت |
| `<TextBox>` | `<صندوق_نص>` | صندوق نص (إدخال) |
| `<RichTextBox>` | `<صندوق_نص_غني>` أو `<نص_غني>` | صندوق نص غني |
| `<Label>` | `<تسمية>` | تسمية |

### أزرار (Buttons)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Button>` | `<زر>` | زر |
| `<RepeatButton>` | `<زر_تكرار>` | زر تكرار |
| `<ToggleButton>` | `<زر_تبديل>` | زر تبديل |
| `<CheckBox>` | `<مربع_اختيار>` | مربع اختيار |
| `<RadioButton>` | `<زر_خيار>` | زر خيار |

### قوائم (Lists)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<ListBox>` | `<صندوق_قائمة>` | صندوق قائمة |
| `<ListView>` | `<عرض_قائمة>` | عرض قائمة |
| `<ComboBox>` | `<صندوق_مدمج>` | صندوق مدمج |
| `<TreeView>` | `<عرض_شجري>` | عرض شجري |
| `<Menu>` | `<قائمة>` | قائمة |
| `<MenuItem>` | `<عنصر_قائمة>` | عنصر قائمة |
| `<ContextMenu>` | `<قائمة_سياق>` | قائمة سياق |
| `<TabControl>` | `<تحكم_تبويب>` | تحكم تبويب |
| `<TabItem>` | `<تبويب>` | تبويب |

### وسائط (Media)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Image>` | `<صورة>` | صورة |
| `<MediaElement>` | `<عنصر_وسائط>` أو `<وسائط>` | عنصر وسائط (فيديو/صوت) |

### إدخال (Input)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Slider>` | `<منزلق>` | منزلق |
| `<ProgressBar>` | `<شريط_تقدم>` | شريط تقدم |
| `<DatePicker>` | `<منتقي_تاريخ>` | منتقي تاريخ |
| `<Calendar>` | `<تقويم>` | تقويم |
| `<PasswordBox>` | `<صندوق_كلمة_سر>` | صندوق كلمة سر |

### أشكال (Shapes)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Rectangle>` | `<مستطيل>` | مستطيل |
| `<Ellipse>` | `<قطع_ناقص>` أو `<ناقص>` | قطع ناقص |
| `<Line>` | `<خط>` | خط |
| `<Polygon>` | `<مضلع>` | مضلع |
| `<Polyline>` | `<خط_متعدد>` | خط متعدد |
| `<Path>` | `<مسار>` | مسار |

### نافذة (Window)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Window>` | `<نافذة>` | نافذة |

### ثلاثي الأبعاد (3D)

| XML (إنجليزي) | XML (عربي) | 설명 |
|--------------|-----------|------|
| `<Viewport3D>` | `<منظور3d>` | منظر ثلاثي الأبعاد |

---

## 3. خصائص العناصر (Attributes)

### المحتوى والنص (Content & Text)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `المحتوى` | `Content` | `Content="OK"` |
| `النص` | `Text` | `Text="Hello"` |
| `الاسم` | `Name` | `Name="btn1"` |
| `المعرف` | `Name` | (مرادف لـ `الاسم`) |

### الأبعاد (Dimensions)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `العرض` | `Width` | `Width="200"` |
| `الارتفاع` | `Height` | `Height="100"` |

### التخطيط (Layout)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `الهامش` | `Margin` | `Margin="5"` |
| `الحشوة` | `Padding` | `Padding="10"` |
| `محاذاة` | `HorizontalAlignment` | |
| `محاذاة_عمودية` | `VerticalAlignment` | |
| `توجيه` | `Orientation` | `Orientation="Horizontal"` |

### المظهر (Appearance)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `الخلفية` | `Background` | `Background="Blue"` |
| `الأمامية` | `Foreground` | `Foreground="White"` |
| `لون` | `Foreground` | (مرادف لـ `الأمامية`) |
| `لون_الخلفية` | `Background` | (مرادف لـ `الخلفية`) |
| `رؤية` | `Visibility` | `Visibility="Hidden"` |
| `مفعل` | `IsEnabled` | `IsEnabled="true"` |
| `محدد` | `IsChecked` | `IsChecked="true"` |

### الخط (Font)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `حجم_الخط` | `FontSize` | `FontSize="24"` |
| `نوع_الخط` | `FontFamily` | `FontFamily="Arial"` |
| `غامق` | `FontWeight` | `FontWeight="Bold"` |
| `مائل` | `FontStyle` | `FontStyle="Italic"` |
| `زخرفة` | `TextDecoration` | `TextDecoration="Underline"` |

### الحدود (Border)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `سمك_الحدود` | `BorderThickness` | `BorderThickness="2"` |
| `لون_الحدود` | `BorderBrush` | `BorderBrush="Red"` |

### الأشكال (Shape)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `تعبئة` | `Fill` | `Fill="Red"` |
| `حد` | `Stroke` | `Stroke="Black"` |

### القيمة والمدى (Value & Range)

| العربية | الإنجليزية | مثال |
|---------|------------|-------|
| `القيمة` | `Value` | `Value="50"` |
| `الحد_الأدنى` | `Minimum` | `Minimum="0"` |
| `الحد_الأقصى` | `Maximum` | `Maximum="100"` |
| `المصدر` | `Source` | `Source="video.mp4"` |

### الأوامر (Commands)

| العربية | الإنجليزية |
|---------|------------|
| `أمر` | `Command` |
| `وسيط_أمر` | `CommandParameter` |

---

## 4. ربط الأحداث (Event Binding)

الخصائص التي تبدأ بحرف كبير (Uppercase) وقيمتها معرف (identifier) تعتبر أحداثاً وتربط باستخدام `=>`:

```xml
<Button Content="OK" Click="handleClick()" />
```

يتحول إلى:

```
a+ new button __xaml_0
__xaml_0.Content = "OK"
__xaml_0.Click => handleClick()
```


### قائمة الأحداث (Events)

| العربية | الإنجليزية | 설명 |
|---------|------------|------|
| `نقر` | `Click` | عند النقر |
| `تحميل` | `Loaded` | عند التحميل |
| `دخول_فأرة` | `MouseEnter` | دخول الفأرة |
| `خروج_فأرة` | `MouseLeave` | خروج الفأرة |
| `ضغط_فأرة` | `MouseDown` | ضغط زر الفأرة |
| `رفع_فأرة` | `MouseUp` | رفع زر الفأرة |
| `تحريك_فأرة` | `MouseMove` | تحريك الفأرة |
| `ضغط_مفتاح` | `KeyDown` | ضغط مفتاح |
| `رفع_مفتاح` | `KeyUp` | رفع مفتاح |
| `اكتسب_تركيز` | `GotFocus` | اكتسب التركيز |
| `فقد_تركيز` | `LostFocus` | فقد التركيز |
| `تحديث_تخطيط` | `LayoutUpdated` | تحديث التخطيط |
| `تغير_نص` | `TextChanged` | تغير النص |
| `تغير_اختيار` | `SelectionChanged` | تغير الاختيار |
| `تغير_قيمة` | `ValueChanged` | تغير القيمة |
| `تغير_حجم` | `SizeChanged` | تغير الحجم |
| `تم_تحديد` | `Checked` | تم التحديد |
| `تم_إلغاء` | `Unchecked` | تم الإلغاء |
| `غير_محدد` | `Indeterminate` | غير محدد |
| `إفلات` | `Drop` | إفلات (سحب وإفلات) |
| `سحب_فوق` | `DragOver` | سحب فوق |

### مثال ربط الأحداث بالعربية:

```xml
<زر المحتوى="اضغطني" نقر="handleClick()" تحميل="onLoad()" />
```

يتحول إلى:

```
a+ new زر __xaml_0
__xaml_0.Content = "اضغطني"
__xaml_0.Click => handleClick()
__xaml_0.Loaded => onLoad()
```

---

## 5. ربط البيانات (Data Binding with `{Binding}`)

تستخدم `{Binding expr}` لربط خاصية بتعبير A+:

```xml
<TextBlock Text="{Binding appCounter}" FontSize="48" />
```

يتحول إلى:

```
a+ new textblock __xaml_0
__xaml_0.Text => appCounter
__xaml_0.FontSize = "48"
```

هذا ينشئ رابطاً يعيد تقييم `appCounter` عند كل تحديث للواجهة.

---

## 6. أمثلة كاملة

### مثال 1: عداد بسيط

```
let appCounter = 0

<StackPanel>
    <TextBlock Text="{Binding appCounter}" FontSize="48" />
    <Button Content="+" Width="100" Click="incClick()" />
    <Button Content="-" Width="100" Click="decClick()" />
    <Button Content="Reset" Width="100" Click="resetClick()" />
</StackPanel>

func incClick() { appCounter = appCounter + 1 }
func decClick() { appCounter = appCounter - 1 }
func resetClick() { appCounter = 0 }

show()
```

### مثال 2: بالعربية كاملة

```
<مكدسة العرض="400" الارتفاع="300">
    <نص النص="مرحباً في A+!" حجم_الخط="28" لون="#0066CC" />
    <زر المحتوى="تشغيل" العرض="100" نقر="handleRun()" />
    <صندوق_نص النص="اكتب هنا..." العرض="300" />
</مكدسة>

func handleRun() {
    print("تم التشغيل")
}

اعرض()
```

### مثال 3: نموذج تسجيل دخول

```
<StackPanel Width="350" Padding="20">
    <TextBlock Text="تسجيل الدخول" FontSize="28" FontWeight="Bold" HorizontalAlignment="Center" />
    <TextBlock Text="اسم المستخدم:" Margin="0,20,0,0" />
    <TextBox Name="usernameInput" Width="300" />
    <TextBlock Text="كلمة السر:" Margin="0,10,0,0" />
    <PasswordBox Name="passwordInput" Width="300" />
    <Button Content="دخول" Width="150" Height="40" Margin="0,20,0,0" Click="loginClick()" />
    <TextBlock Name="statusLabel" Text="" Margin="0,10,0,0" Foreground="Red" />
</StackPanel>

func loginClick() {
    if (usernameInput.text == "admin" and passwordInput.text == "1234") {
        statusLabel.Text = "مرحباً يا مدير!"
        statusLabel.Foreground = "Green"
    } else {
        statusLabel.Text = "خطأ في اسم المستخدم أو كلمة السر"
    }
}

show()
```

### مثال 4: وسائط (MediaElement)

```
<StackPanel Width="600" Height="400">
    <MediaElement Name="player" Source="intro.mp4" Width="560" Height="300" Margin="20,10" />
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
        <Button Content="▶ تشغيل" Width="100" Click="player.play()" />
        <Button Content="⏸ إيقاف مؤقت" Width="100" Click="player.pause()" />
        <Button Content="⏹ إيقاف" Width="100" Click="player.stop()" />
    </StackPanel>
</StackPanel>
show()
```

### مثال 5: 3D مع Viewport3D

```
<Viewport3D Width="500" Height="400">
</Viewport3D>
let cam = new Camera(0, 0, 5)
let light = new Light(0, -1, 1)
let obj = new Object(0, 0, 1)

viewport3d new vp
vp.Camera = cam
vp.Light = light
vp.Object = obj
show()
```

---

## 7. دوال عناصر واجهة المستخدم (UI Methods)

### add / أضف
إضافة عنصر ابن:

```
parent.add(child)
panel.add(btn)
```

### remove / احذف
إزالة عنصر ابن:

```
parent.remove(child)
panel.remove(btn)
```

### play / شغل, pause / أوقف, stop / إيقاف
التحكم في MediaElement:

```
player.play()
player.شغل()
player.pause()
player.أوقف()
player.stop()
player.إيقاف()
```

### أحداث MediaElement

```
player.MediaEnded = fn() { print("انتهى التشغيل") }
player.انتهى_الوسائط = fn() { print("انتهى التشغيل") }
player.MediaFailed = fn() { print("فشل التشغيل") }
player.فشل_الوسائط = fn() { print("فشل التشغيل") }
player.MediaOpened = fn() { print("تم فتح الوسائط") }
player.فتح_الوسائط = fn() { print("تم فتح الوسائط") }
```

---

## 8. اختصارات سريعة (Shorthand)

| الاختصار | المعنى |
|---------|--------|
| `م>` | `let` (متغير) |
| `ط>` | `print(` (طباعة) |
| `لو>` | `if(` (شرط) |
| `ت>` | `while(` (حلقة) |
| `?>شرط_أمر1_أمر2_` | `if(شرط) { أمر1 } else { أمر2 }` |
| `} _ {` | `} else {` |
| `x++` | `x += 1` |
| `x--` | `x -= 1` |

---

## 9. أمثلة سريعة

**أبسط تطبيق:**
```
<TextBlock Text="Hello A+" FontSize="32" />
show()
```

**زر واحد:**
```
<StackPanel>
    <Button Content="انقرني" Click="sayHello()" />
</StackPanel>
func sayHello() { print("مرحباً!") }
show()
```

**صورة:**
```
<Img Source="https://example.com/logo.png" Width="200" />
show()
```

**بالعربية الكاملة:**
```
<نص النص="أهلاً" حجم_الخط="24" />
اعرض()
```
