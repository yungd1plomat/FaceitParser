# Введение

Проект написан на C# NET 6.0, используется EF6, БД mysql 8.0.x.
При желании можно поменять БД, для этого установите другое расширение вместо Pomelo.Mysql в проекте
Запускается проект на порте 3810, поменять можете в файле Dockerfile (заменить все 3810 на свой порт)

# Установка

## Вообще с нуля (с сурсов)

Установите Net 6.0 SDK 
Необходимо собрать проект и запушить в dockerhub.
Для этого поставьте docker на свою систему и настройке учетную запись.
Далее перейдите в каталог с проектом (./FaceitParser) и соберите образ при помощи команды

```bash
docker build -t никучетки/faceitparser -f FaceitParser/Dockerfile . 
```

Далее запушим образ в dockerhub

```bash
docker push никучетки/faceitparser
```

## Установка и настройка Mysql

### Установка Mysql Server

```bash
sudo apt update
sudo apt install mysql-server
```

### Настройка Mysql 

Подлючаемся к mysql командой
```bash
sudo mysql
```

Далее меняем метод аутентификации рута и пароль
```mysql
ALTER USER 'root'@'localhost' IDENTIFIED WITH caching_sha2_password BY 'psswd';
FLUSH PRIVILEGES;
```
Где **'psswd'** ваш пароль

Cоздаем нового пользователя, например я обычно создаю admin
```mysql
CREATE USER 'admin'@'%' IDENTIFIED BY 'psswd';
```
Где **'psswd'** ваш пароль.

**Важно, всегда создаем пользователя с удаленным подключением, т.е после @ всегда идет %, а не localhost, иначе подключиться не получится**

Даем права пользователю на свое бд (faceit в моем случае)
```mysql
GRANT ALL PRIVILEGES ON faceit.* TO 'admin'@'%' WITH GRANT OPTION;
```

Выходим командой 
```bash
exit
```

Разрешаем удаленные подключения, для этого открываем файл по пути
```
/etc/mysql/mysql.conf.d/mysqld.cnf
```

Можете редачить его через любой ftp клиент.
Я обычно редачу через nano командой
```bash
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf
```
Перемещаемся при помощи стрелочек в консоли.

Удаляем строку 
```
bind-address = 127.0.0.1
```

Сохраняем, для этого нажимаем ctrl + s и дальше ctrl + x

Перезапускаем Mysql 
```bash
sudo systemctl restart mysql
```

На этом настройка mysql завершена.

## Установка и настройка Docker

Дальше будет тоже самое с [оффициальной инструкции](https://docs.docker.com/engine/install/ubuntu/) на момент написании данной статьи

### Настройка перед установкой

```bash
sudo apt-get update
sudo apt-get install \
    ca-certificates \
    curl \
    gnupg \
    lsb-release
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```

### Установка docker engine

```bash
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-compose-plugin
```

Проверить правильно ли установлен Docker можете командой
```bash
sudo docker run hello-world
```

## Установка и настройка Nginx

### Установка

```bash
sudo apt-get install nginx
sudo service nginx start
```

### Настройка Proxy

Нам необходимо перенаправить входящий http траффик на наш внутренний локалхост.

Для этого редактируем файл
```
/etc/nginx/sites-available/default
```

Я буду делать это опять таки при помощи nano
```bash
sudo nano /etc/nginx/sites-available/default
```

Все стираем и вставляем такое содержимое
```nginx
server {
    listen        80;
    server_name   IP/DOMAIN NAME;
	client_max_body_size 40M;
	proxy_connect_timeout 600;
	proxy_send_timeout 600;
	proxy_read_timeout 600;
	send_timeout 600;
    location / {
        proxy_pass         http://localhost:3810;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Где IP/DOMAIN NAME - домен который вы подвязали. Если вы не подвязывали домен к серверу, то просто удалите эту строку.

Если вы меняли дефолтный порт, то измение адрес http://localhost:3810 на адрес со своим портом.

Проверим, валидная ли конфигурация командой
```bash
sudo nginx -t
```

Если выдало ошибку возвращайтесь к предыдущему шагу

Перезапускаем nginx 
```bash
sudo nginx -s reload
```

### Настройка SSL
Юзаем Letencrypt сертификат, для настройки используем Certbot

Устанавливаем
```bash
sudo apt-get install python3-certbot-nginx
```

Генерируем новый сертификат, для этого используем команду
```bash
sudo certbot --nginx -d domain.com
```

Где **domain.com** - ваш домен.

В первом поле пишем email, который будет отображаться как контактная информация, можем вписать любую почту.

Когда спросит
```
Please choose whether or not to redirect HTTP traffic to HTTPS, removing HTTP access.
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
1: No redirect - Make no further changes to the webserver configuration.
2: Redirect - Make all requests redirect to secure HTTPS access. Choose this for
new sites, or if you're confident your site works on HTTPS. You can undo this
change by editing your web server's configuration.
```

Выбираем 2 вариант, чтобы http автоматически перенаправлялся на https.

## Настройка и запуск самого сайта

### Конфигурационный файл

Создаем файл с любым расширением и названием, например я создал env.list и пишем туда такое содержимое
```
CONNECTION_STRING=Server=1.1.1.1;Port=3306;Database=faceit;Uid=admin;Pwd=psswd;
STEAM_API_KEY=mysteamapikey123_fdsfdsfsdf-1234
```

**1.1.1.1** - айпи вашего сервера

**faceit** - название бд, к которому вы дали доступ

**admin** - имя пользователя которого вы создали

**psswd** - пароль пользователя

**mysteamapikey123_fdsfdsfsdf-1234** - API Key от steamapis.com

Закидываем данный файл на сервер, откуда будем запускать команду (дефолтная папка root)

Подключаемся к серверу и пишем команду
```bash
docker run --restart=always --env-file env.list -itd -p 3810:3810 yungd1plomat/faceitparser
```

**env.list** - путь к вашему конфигурационному файлу

**yungd1plomat/faceitparser** - репозиторий из dockerhub, который вы запушили.

*Можете использовать мой репозиторий который указан в команде*

Готово, на этом мы поставили проект на сервер

# Проблемы

При возникновении проблем пишите мне в [Телеграм](https://t.me/yungd1plomat)
