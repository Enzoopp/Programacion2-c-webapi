@echo off
echo ========================================
echo   LIMPIANDO BASE DE DATOS BANKLINK
echo ========================================
echo.

cd /d "%~dp0"

echo [1/3] Borrando base de datos...
dotnet ef database drop --force

echo.
echo [2/3] Recreando base de datos...
dotnet ef database update

echo.
echo [3/3] Verificando...
sqlcmd -S ".\SQLEXPRESS" -d BankLinkDb -Q "SELECT COUNT(*) AS Clientes FROM Clientes; SELECT COUNT(*) AS Cuentas FROM Cuentas"

echo.
echo ========================================
echo   BASE DE DATOS LIMPIA Y LISTA!
echo ========================================
echo.
echo Presiona cualquier tecla para cerrar...
pause > nul
