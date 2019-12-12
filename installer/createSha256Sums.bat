@echo off
if exist shasums.txt (
    del shasums.txt
    ) else (
    type nul > shasums.txt
)

for /R %cd% %%A in (*.*) do certutil -hashfile "%%~fA" SHA1 >> shasums.txt
