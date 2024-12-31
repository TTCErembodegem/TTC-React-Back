FROM mysql:5.5.60

COPY *.sql /docker-entrypoint-initdb.d/
RUN chmod 644 /docker-entrypoint-initdb.d/*.sql

CMD ["mysqld"]
