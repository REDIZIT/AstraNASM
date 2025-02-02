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