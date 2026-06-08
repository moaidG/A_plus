# Async/Await/Spawn (البرمجة غير المتزامنة)

يدعم A+ البرمجة غير المتزامنة (asynchronous) لتنفيذ العمليات دون حظر.

## Async Function (دالة غير متزامنة)

```
async func fetchData() {
    return 42
}
```

الدوال غير المتزامنة تُنفّذ عبر `Task.Run()` وتعيد `Task<object>`.

## Await (انتظار)

```
async func work() {
    let result = await fetchData()
    print("Result: " + result)
}
```

`await` ينتظر إكمال المهمة غير المتزامنة ويحصل على النتيجة.

## Go (إطلاق غير متزامن بدون انتظار)

```
go fetchData()
print("هذا يطبع فوراً دون انتظار")
```

`go` يشعل مهمة في الخلفية ولا ينتظرها (fire-and-forget).

## Spawn (إنشاء مهمة خلفية)

```
spawn backgroundTask(n) {
    print("بدأت المهمة: " + n)
    return n * 2
}

let r = await backgroundTask(10)
print("النتيجة: " + r)
```

`spawn` يسجل دالة غير متزامنة يمكن استدعاؤها لاحقاً.
