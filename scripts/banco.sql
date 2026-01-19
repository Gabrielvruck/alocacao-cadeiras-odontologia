CREATE DATABASE IF NOT EXISTS alocacao_cadeiras;
USE alocacao_cadeiras;

CREATE TABLE IF NOT EXISTS cadeiras (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numero INT NOT NULL,
    descricao VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS alocacoes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cadeira_id INT NOT NULL,
    data_hora_inicio DATETIME NOT NULL,
    data_hora_fim DATETIME NOT NULL,
    CONSTRAINT fk_alocacoes_cadeiras FOREIGN KEY (cadeira_id) REFERENCES cadeiras(id)
);

INSERT INTO cadeiras (numero, descricao)
VALUES
    (1, 'Cadeira odontológica premium'),
    (2, 'Cadeira com encosto reclinável'),
    (3, 'Cadeira compacta para consultório');

INSERT INTO alocacoes (cadeira_id, data_hora_inicio, data_hora_fim)
VALUES
    (1, '2024-01-10 08:00:00', '2024-01-10 09:00:00'),
    (2, '2024-01-10 09:00:00', '2024-01-10 10:00:00'),
    (3, '2024-01-10 10:00:00', '2024-01-10 11:00:00');
