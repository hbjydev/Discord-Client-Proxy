# Nginx configuration

This is an example of the nginx configuration file.
Add this in the http block in /etc/nginx/nginx.conf.

```nginx
server {
	server_name dcp.localhost;
	listen 80;

	location /api/ {
		proxy_pass http://127.0.0.1:3001; #Your fosscord server
		proxy_set_header Host $host;
		proxy_pass_request_headers      on;
		add_header Last-Modified $date_gmt;
		add_header Cache-Control 'no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0';
		proxy_set_header  X-Real-IP $remote_addr;
		proxy_set_header  X-Forwarded-Proto https;
		proxy_set_header  X-Forwarded-For $remote_addr;
		proxy_set_header  X-Forwarded-Host $remote_addr;
		proxy_no_cache 1;
		proxy_cache_bypass 1;
		proxy_set_header Upgrade $http_upgrade;
		proxy_set_header Connection "upgrade";
	}
	location / {
		proxy_pass http://127.0.0.1:5233; #Test client proxy
		proxy_set_header Host $host;
		proxy_pass_request_headers      on;
		add_header Last-Modified $date_gmt;
		add_header Cache-Control 'no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0';
		proxy_set_header  X-Real-IP $remote_addr;
		proxy_set_header  X-Forwarded-Proto https;
		proxy_set_header  X-Forwarded-For $remote_addr;
		proxy_set_header  X-Forwarded-Host $remote_addr;
		proxy_no_cache 1;
		proxy_cache_bypass 1;
		proxy_set_header Upgrade $http_upgrade;
		proxy_set_header Connection "upgrade";
	}
    #Used during development to provide a static global_env while fosscord-server-ts does not support this.
#	location /api/_fosscord/v1/global_env {
#		root /wwwroot;
#		rewrite ^  /global_env break;
#		index global_env;
#	}
}
```
