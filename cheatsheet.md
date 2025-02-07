# Компиляция и запуск в VirtualBox

1. Запустить Ubuntu

2. Скопировать скомпилированный example.nasm в \\wsl.localhost\Ubuntu\home\redizit\Experiments\AstraNASM\source\example.nasm

3. В Ubuntu: 
cd Experiments/AstraNASM
./makeiso.sh

4. Запустить ConEmu:
cd C:\Program Files\Oracle\VirtualBox
VirtualBoxVM.exe --dbg --startvm AstraNASM


# Дамп физической памяти ОЗУ из VirtualBox

cd C:\Program Files\Oracle\VirtualBox

VirtualBoxVM.exe --dbg --startvm AstraNASM

Меню -> Отладка -> Командная строка
```
.pgmphystofile 123.raw
```

Файл 123.raw находится в папке с VirtualBoxVM.exe


# Создание .iso образа

```
nasm -f elf64 source/multiboot.asm -o obj/multiboot.o
nasm -f elf64 source/kernel.asm -o obj/kernel.o
nasm -f elf64 source/example.nasm -o obj/example.o

ld -m elf_x86_64 -T linker.ld -o iso/boot/kernel.bin obj/kernel.o obj/multiboot.o obj/example.o

grub-mkrescue -o myos_nasm.iso iso/
```

# Memory map
https://wiki.osdev.org/Memory_Map_(x86)

Загрузочный сектор: 0x7c00-0x7dff
Свободная память: 0x100000-...

Адрес VGA: 0xB8000

# GRUB
GRUB загружает kernel по адресу 1M = 0x100000
GRUB передаёт управление kernel именно по адресу 1M

Однако GRUB засерает память копиями kernel в разных местах:
1. 0x6e800 - (0x7E00 - 0x7FFFF) 480.5 KiB - Conventional memory
2. 0x100000 - RAM -- free for use (if it exists) - Extended memory
3. 0x3e486a0, 0x3e48... - загадочные высокие адреса, в теории, можно перезаписывать

RBP находится в районе 0x07fef8 адреса

GRUB загружает kernel в protected mode (CR0.PE = 1)



# BOOTBOOT

Загрузчик по типу GRUB, но позволяет загружаться сразу в Long mode (64 битном)
https://gitlab.com/bztsrc/bootboot

Как использовать:

1. Скомпилировать ядро (из c++ или nasm) в elf64 в object файлы (.o)


2. Запустить Makefile или сделать ручками
2.1. ld -r -b binary -o font.o font.psf - добавить стандартный шрифт
2.2. ld -nostdlib -n -T link.ld kernel.o font.o - слинковать ядро и шрифт вместе по скрипту линковщика
2.3. strip -s -K mmio -K fb -K bootboot -K environment -K initstack mykernel.x86_64.elf - зачем-то обрезать???

Скрипт линковщика:
ENTRY(main)
```
mmio        = 0xfffffffff8000000;              /* these are configurable for level 2 loaders */
fb          = 0xfffffffffc000000;
bootboot    = 0xffffffffffe00000;
environment = 0xffffffffffe01000;
/* initstack = 1024; */
PHDRS
{
  boot PT_LOAD;                                /* one single loadable segment */
}
SECTIONS
{
    . = 0xffffffffffe02000;
    .text : {
        KEEP(*(.text.boot)) *(.text .text.*)   /* code */
        *(.rodata .rodata.*)                   /* data */
        *(.data .data.*)
    } :boot
    .bss (NOLOAD) : {                          /* bss */
        . = ALIGN(16);
        *(.bss .bss.*)
        *(COMMON)
    } :boot

    /DISCARD/ : { *(.eh_frame) *(.comment) }
}
```

3. После всего этого должен получится файлик mykernel.x86_64.elf - **Ядро системы готово к объединению с BOOTBOOT**
Из-за линковщика этот файл будет иметь необычную структуру секций. Без этой структуры BOOTBOOT не сможет обработать такое ядро.


4. Скопировать получившийся .elf файл в boot/название_ядра.elf, чтобы шаг #5 сработал


5. В Ubuntu выполнить команду ./mkbootimg cfg.json myos.img
mkbootimg надо скачать из репозитория BOOTBOOT
В cfg.json надо написать:
{
	"config": "env.txt",
	"initrd": { "type": "tar", "gzip": false, "directory": "boot" },
	"partitions": [
        { "type": "boot", "size": 16 }
    ]
}

Где env.txt - это конфигурация самого BOOTBOOT:
screen=800x600
kernel=mykernel.x86_64.elf

Где mykernel.x86_64.elf - это boot/mykernel.x86_64.elf


6. Теперь у нас есть myos.img
Это сырой образ операционной системы. Этот файл уже можно запустить на виртуальной машине, но только на QEMU.
В VirtualBox этот файл не запуститься, так как QEMU делает несколько скрытых шагов для запуска, а для VMBox нам надо проделать эти шаги вручную.


7. Запуск в VirtualBox

В Ubuntu:
```
VBoxManage convertfromraw --format VDI myos.img myos.vdi --uuid=2a5b2242-ead3-4f96-9663-159554846c48
```

В VirtualBox открываем окно нашей машины -> Настроить -> Носители -> Контроллер IDE -> Добавить жесткий диск -> Выбрать myos.vdi

