# Дамп физической памяти ОЗУ из VirtualBox

cd C:\Program Files\Oracle\VirtualBox

VirtualBoxVM.exe --dbg --startvm AstraNASM

Меню -> Отладка -> Командная строка
.pgmphystofile 123.raw

Файл 123.raw находится в папке с VirtualBoxVM.exe


# Создание .iso образа

nasm -f elf64 source/multiboot.asm -o obj/multiboot.o
nasm -f elf64 source/kernel.asm -o obj/kernel.o
nasm -f elf64 source/example.nasm -o obj/example.o

ld -m elf_x86_64 -T linker.ld -o iso/boot/kernel.bin obj/kernel.o obj/multiboot.o obj/example.o

grub-mkrescue -o myos_nasm.iso iso/


# Memory map
https://wiki.osdev.org/Memory_Map_(x86)

Загрузочный сектор: 0x7c00-0x7dff
Свободная память: 0x100000-...

# GRUB
GRUB загружает kernel по адресу 1M = 0x100000
GRUB передаёт управление kernel именно по адресу 1M

Однако GRUB засерает память копиями kernel в разных местах:
1. 0x6e800 - (0x7E00 - 0x7FFFF) 480.5 KiB - Conventional memory
2. 0x100000 - RAM -- free for use (if it exists) - Extended memory
3. 0x3e486a0, 0x3e48... - загадочные высокие адреса, в теории, можно перезаписывать