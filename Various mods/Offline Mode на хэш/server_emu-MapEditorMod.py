import os
import requests
from flask import Flask, request, Response

# ========================================================
# НАСТРОЙКИ СЕРВЕРА
# ========================================================
# Установите True, чтобы принудительно включить ПОЛНЫЙ ОФФЛАЙН режим.
# Установите False, чтобы сервер пытался докачивать недостающие файлы из интернета.
FORCE_OFFLINE = True

app = Flask(__name__)

# Создаем папки для кэша, сохранений и теперь для пользовательских карт (custom_maps)
for folder in ['cache', 'saves', 'custom_maps']:
    if not os.path.exists(folder):
        os.makedirs(folder)

# Хитрая функция: узнаем настоящий IP сервера разработчиков в обход файла hosts!
def get_real_ip():
    if FORCE_OFFLINE:
        return None # Если включен полный оффлайн, даже не ищем IP в интернете
    
    try:
        res = requests.get('https://dns.google/resolve?name=st.nobodyshot.com', timeout=5)
        return res.json()['Answer'][0]['data']
    except Exception as e:
        return None

REAL_IP = get_real_ip()

# ========================================================
# ГЕНЕРАТОР БАЗОВОГО ОФФЛАЙН ПРОФИЛЯ
# ========================================================
def generate_starter_profile():
    profile_path = "cache/req_1_map_none.bin"
    if not os.path.exists(profile_path):
        print("[!] Профиль не найден. Генерируем Базовый Оффлайн Профиль...")
        # 999999 - Деньги, 50 - уровень
        fake_profile = "0^1^999999^999999^0^0^0^50^0^0^0^0^0"
        with open(profile_path, 'wb') as f:
            f.write(fake_profile.encode('utf-8'))

# Создаем профиль при старте, если его нет
generate_starter_profile()

# ========================================================
# ПРИЕМ КАРТ ИЗ НАШЕГО НОВОГО МОДА (ИЛИ РЕДАКТОРА)
# ========================================================
@app.route('/upload_custom_map', methods=['POST'])
def upload_custom_map():
    map_id = request.args.get('id', '1')
    filename = f"m{map_id}.bytes"
    filepath = os.path.join('custom_maps', filename)
    
    # ИСПОЛЬЗУЕМ get_data() чтобы гарантированно получить сырые байты!
    raw_data = request.get_data() 
    
    with open(filepath, 'wb') as f:
        f.write(raw_data)
        
    print(f"\n[РЕДАКТОР КАРТ] Получена карта! Размер: {len(raw_data)} байт. Сохранена в: {filepath}\n")
    return "OK", 200

# ========================================================
# СТАНДАРТНАЯ ИГРОВАЯ ЛОГИКА
# ========================================================
@app.route('/', defaults={'path': ''})
@app.route('/<path:path>', methods=['GET', 'POST'])
def catch_all(path):
    req_code = request.values.get('requestCode', 'none')
    map_id = request.values.get('mapid', 'none')
    
    # Имя файла, куда мы сохраним данные
    cache_filename = f"cache/req_{req_code}_map_{map_id}.bin"
    
    # --- СОХРАНЕНИЕ ВСЕХ ИЗМЕНЕНИЙ ИГРОКА (ПОКУПКИ, СТАТИСТИКА) ---
    if request.method == 'POST' and request.data:
        save_filename = f"saves/client_upload_req_{req_code}_map_{map_id}.bin"
        with open(save_filename, 'wb') as f:
            f.write(request.data)
        print(f"[СОХРАНЕНИЕ] Действия/изменения клиента сохранены в {save_filename}")

    # --- ЗАЩИТА ЛОКАЛЬНОГО ПРОФИЛЯ ОТ ПЕРЕЗАПИСИ ---
    if req_code == '1' and map_id == 'none':
        if os.path.exists(cache_filename):
            print(f"[ЛОКАЛ] Отдаем ваш локальный профиль (защищено от офф. сервера): {cache_filename}")
            with open(cache_filename, 'rb') as f:
                return Response(f.read(), status=200, content_type="text/html")

    # ЕСЛИ ЕСТЬ ИНТЕРНЕТ И РЕЖИМ НЕ ОФФЛАЙН: Скачиваем кэш и сохраняем на ПК
    if REAL_IP and not FORCE_OFFLINE:
        url = f"http://{REAL_IP}/{path}"
        headers = {'Host': 'st.nobodyshot.com'}
        
        # Копируем заголовки от игры
        for k, v in request.headers.items():
            if k.lower() != 'host':
                headers[k] = v
                
        try:
            print(f"[ОНЛАЙН] Запрос (requestCode={req_code}) к серверу разработчиков...")
            if request.method == 'GET':
                resp = requests.get(url, params=request.args, headers=headers, timeout=10)
            else:
                resp = requests.post(url, data=request.form, headers=headers, timeout=10)
            
            # СОХРАНЯЕМ В ФАЙЛ, только если это не ошибка подписи (sigError)
            if b'sigError' not in resp.content and resp.status_code == 200:
                with open(cache_filename, 'wb') as f:
                    f.write(resp.content)
                print(f"  -> Ответ успешно сохранен в {cache_filename}")
            
            return Response(resp.content, status=resp.status_code, content_type=resp.headers.get('content-type', 'text/html'))
        except Exception as e:
            print(f"  -> Ошибка соединения с офф. сервером. Пробуем загрузить из кэша...")
    
    # ЕСЛИ ОФФЛАЙН (ИЛИ СЕРВЕР УПАЛ): Отдаем сохраненные файлы!
    if os.path.exists(cache_filename):
        print(f"[ОФФЛАЙН] Отдаем сохраненные данные из файла: {cache_filename}")
        with open(cache_filename, 'rb') as f:
            return Response(f.read(), status=200, content_type="text/html")
    else:
        print(f"[!] ОШИБКА: Файла {cache_filename} нет в кэше!")
        # Возвращаем универсальный ответ Успеха, чтобы игра не крашнулась (бесконечные деньги)
        return "0^1^999999^999999^0^0^0^50^0^0^0^0^0"

if __name__ == '__main__':
    print("==================================================")
    print("ЛОКАЛЬНЫЙ СЕРВЕР С ПОДДЕРЖКОЙ РЕДАКТОРА КАРТ ЗАПУЩЕН!")
    if FORCE_OFFLINE:
        print("РЕЖИМ: ПОЛНЫЙ ОФФЛАЙН (Включено вручную).")
        print("Сервер работает изолированно и не обращается к интернету!")
    elif REAL_IP:
        print(f"РЕЖИМ: ОНЛАЙН (Умный кэш). Реальный IP сервера: {REAL_IP}")
        print("Ваш профиль (req_1) защищен. Все изменения сохраняются в папку saves!")
    else:
        print("РЕЖИМ: ОФФЛАЙН (Нет интернета / Сервер недоступен).")
        print("Чтение данных из папки cache.")
    print("==================================================")
    app.run(host='0.0.0.0', port=80)