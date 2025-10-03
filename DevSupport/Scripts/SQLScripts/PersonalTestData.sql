-- Date de test pentru tabela Personal
INSERT INTO Personal (
    Cod_Angajat, CNP, Nume, Prenume, Data_Nasterii, Locul_Nasterii, 
    Telefon_Personal, Email_Personal, 
    Adresa_Domiciliu, Judet_Domiciliu, Oras_Domiciliu,
    Stare_Civila, Functia, Departament,
    Serie_CI, Numar_CI, Eliberat_CI_De, Data_Eliberare_CI,
    Creat_De
) VALUES 
-- Personal Administrativ
('EMP001', '1234567890123', 'Popescu', 'Maria', '1985-03-15', 'Bucuresti', '0723456789', 'maria.popescu@valyanmed.ro', 'Str. Florilor nr. 15', 'Bucuresti', 'Bucuresti', 'Casatorit/a', 'Administrator Clinic', 'Administratie', 'BX', '123456', 'SPCEP Sector 1', '2020-05-10', 'SYSTEM'),
('EMP002', '2345678901234', 'Ionescu', 'Alexandru', '1980-07-22', 'Cluj-Napoca', '0734567890', 'alex.ionescu@valyanmed.ro', 'Bd. Unirii nr. 25', 'Cluj', 'Cluj-Napoca', 'Necasatorit/a', 'Manager Resurse Umane', 'Administratie', 'CJ', '234567', 'SPCEP Cluj', '2019-08-15', 'SYSTEM'),
('EMP003', '3456789012345', 'Vasile', 'Elena', '1990-12-08', 'Iasi', '0745678901', 'elena.vasile@valyanmed.ro', 'Str. Pacii nr. 8', 'Iasi', 'Iasi', 'Casatorit/a', 'Contabil Sef', 'Financiar', 'IS', '345678', 'SPCEP Iasi', '2021-02-28', 'SYSTEM'),
('EMP004', '4567890123456', 'Popa', 'Mihai', '1988-01-30', 'Timisoara', '0756789012', 'mihai.popa@valyanmed.ro', 'Str. Libertatii nr. 12', 'Timis', 'Timisoara', 'Divortat/a', 'IT Manager', 'IT', 'TM', '456789', 'SPCEP Timis', '2020-11-05', 'SYSTEM'),
('EMP005', '5678901234567', 'Stan', 'Andreea', '1992-06-18', 'Constanta', '0767890123', 'andreea.stan@valyanmed.ro', 'Bd. Mamaia nr. 45', 'Constanta', 'Constanta', 'Necasatorit/a', 'Specialist Marketing', 'Marketing', 'CT', '567890', 'SPCEP Constanta', '2022-01-15', 'SYSTEM'),

-- Personal Financiar
('EMP006', '6789012345678', 'Radu', 'Cristina', '1987-04-12', 'Brasov', '0778901234', 'cristina.radu@valyanmed.ro', 'Str. Republicii nr. 22', 'Brasov', 'Brasov', 'Casatorit/a', 'Contabil', 'Financiar', 'BV', '678901', 'SPCEP Brasov', '2021-06-20', 'SYSTEM'),
('EMP007', '7890123456789', 'Marin', 'George', '1983-09-25', 'Galati', '0789012345', 'george.marin@valyanmed.ro', 'Bd. Brailei nr. 18', 'Galati', 'Galati', 'Necasatorit/a', 'Specialist Facturare', 'Financiar', 'GL', '789012', 'SPCEP Galati', '2020-03-18', 'SYSTEM'),
('EMP008', '8901234567890', 'Tudor', 'Ioana', '1991-11-14', 'Sibiu', '0790123456', 'ioana.tudor@valyanmed.ro', 'Str. Avram Iancu nr. 5', 'Sibiu', 'Sibiu', 'Casatorit/a', 'Specialist Buget', 'Financiar', 'SB', '890123', 'SPCEP Sibiu', '2022-04-10', 'SYSTEM'),

-- Personal IT
('EMP009', '9012345678901', 'Dima', 'Razvan', '1989-08-03', 'Pitesti', '0701234567', 'razvan.dima@valyanmed.ro', 'Str. Victoriei nr. 30', 'Arges', 'Pitesti', 'Necasatorit/a', 'Developer Senior', 'IT', 'AG', '901234', 'SPCEP Arges', '2021-09-15', 'SYSTEM'),
('EMP010', '0123456789012', 'Nistor', 'Monica', '1986-02-28', 'Ploiesti', '0712345678', 'monica.nistor@valyanmed.ro', 'Bd. Republicii nr. 40', 'Prahova', 'Ploiesti', 'Divortat/a', 'Analyst Sistem', 'IT', 'PH', '012345', 'SPCEP Prahova', '2020-07-22', 'SYSTEM'),
('EMP011', '1123456789012', 'Florea', 'Adrian', '1984-05-16', 'Craiova', '0723456780', 'adrian.florea@valyanmed.ro', 'Str. Unirii nr. 12', 'Dolj', 'Craiova', 'Casatorit/a', 'Administrator Retea', 'IT', 'DJ', '112345', 'SPCEP Dolj', '2019-12-08', 'SYSTEM'),

-- Personal Receptie si Relatii Clienti
('EMP012', '2234567890123', 'Cristea', 'Raluca', '1993-10-07', 'Oradea', '0734567891', 'raluca.cristea@valyanmed.ro', 'Str. Independentei nr. 7', 'Bihor', 'Oradea', 'Necasatorit/a', 'Receptioner', 'Receptie', 'BH', '223456', 'SPCEP Bihor', '2022-08-12', 'SYSTEM'),
('EMP013', '3345678901234', 'Pavel', 'Diana', '1988-12-19', 'Arad', '0745678902', 'diana.pavel@valyanmed.ro', 'Bd. Revolutiei nr. 35', 'Arad', 'Arad', 'Casatorit/a', 'Specialist Relatii Clienti', 'Relatii Clienti', 'AR', '334567', 'SPCEP Arad', '2021-03-25', 'SYSTEM'),
('EMP014', '4456789012345', 'Lazar', 'Bogdan', '1990-07-11', 'Deva', '0756789013', 'bogdan.lazar@valyanmed.ro', 'Str. Decebal nr. 20', 'Hunedoara', 'Deva', 'Necasatorit/a', 'Coordonator Programari', 'Receptie', 'HD', '445678', 'SPCEP Hunedoara', '2022-01-30', 'SYSTEM'),

-- Personal Curatenie si Intretinere
('EMP015', '5567890123456', 'Gheorghe', 'Vasile', '1975-03-22', 'Alba Iulia', '0767890124', 'vasile.gheorghe@valyanmed.ro', 'Str. Horea nr. 14', 'Alba', 'Alba Iulia', 'Casatorit/a', 'Supervisor Curatenie', 'Intretinere', 'AB', '556789', 'SPCEP Alba', '2018-11-12', 'SYSTEM'),
('EMP016', '6678901234567', 'Matei', 'Ana', '1982-01-08', 'Targu Mures', '0778901235', 'ana.matei@valyanmed.ro', 'Str. Primaverii nr. 9', 'Mures', 'Targu Mures', 'Vaduv/a', 'Ingrijitor Cladire', 'Intretinere', 'MS', '667890', 'SPCEP Mures', '2019-06-18', 'SYSTEM'),
('EMP017', '7789012345678', 'Barbu', 'Ion', '1978-09-05', 'Resita', '0789012346', 'ion.barbu@valyanmed.ro', 'Bd. Muncii nr. 25', 'Caras-Severin', 'Resita', 'Casatorit/a', 'Tehnician Intretinere', 'Intretinere', 'CS', '778901', 'SPCEP Caras-Severin', '2020-02-14', 'SYSTEM'),

-- Personal Securitate
('EMP018', '8890123456789', 'Neagu', 'Marius', '1981-06-30', 'Slatina', '0790123457', 'marius.neagu@valyanmed.ro', 'Str. Libertarii nr. 18', 'Olt', 'Slatina', 'Necasatorit/a', 'Agent Securitate', 'Securitate', 'OT', '889012', 'SPCEP Olt', '2021-05-08', 'SYSTEM'),
('EMP019', '9901234567890', 'Preda', 'Stefan', '1985-04-17', 'Ramnicu Valcea', '0701234568', 'stefan.preda@valyanmed.ro', 'Str. Tudor Vladimirescu nr. 33', 'Valcea', 'Ramnicu Valcea', 'Casatorit/a', 'Sef Securitate', 'Securitate', 'VL', '990123', 'SPCEP Valcea', '2019-10-25', 'SYSTEM'),

-- Personal Magazie si Logistica
('EMP020', '0012345678901', 'Savu', 'Florin', '1987-11-23', 'Buzau', '0712345679', 'florin.savu@valyanmed.ro', 'Bd. Nicolae Balcescu nr. 15', 'Buzau', 'Buzau', 'Divortat/a', 'Magaziner', 'Logistica', 'BZ', '001234', 'SPCEP Buzau', '2020-09-30', 'SYSTEM'),
('EMP021', '1123456789013', 'Oprea', 'Laura', '1989-08-14', 'Focsani', '0723456781', 'laura.oprea@valyanmed.ro', 'Str. Cuza Voda nr. 28', 'Vrancea', 'Focsani', 'Casatorit/a', 'Specialist Achizitii', 'Logistica', 'VN', '112346', 'SPCEP Vrancea', '2021-07-19', 'SYSTEM'),

-- Personal Administrativ Senior
('EMP022', '2234567890124', 'Marinescu', 'Radu', '1979-05-12', 'Barlad', '0734567892', 'radu.marinescu@valyanmed.ro', 'Str. Stefan cel Mare nr. 45', 'Vaslui', 'Barlad', 'Casatorit/a', 'Director Administrativ', 'Administratie', 'VS', '223457', 'SPCEP Vaslui', '2017-03-15', 'SYSTEM'),
('EMP023', '3345678901235', 'Tanase', 'Mihaela', '1983-10-28', 'Slobozia', '0745678903', 'mihaela.tanase@valyanmed.ro', 'Bd. Matei Basarab nr. 12', 'Ialomita', 'Slobozia', 'Necasatorit/a', 'Manager Calitate', 'Calitate', 'IL', '334568', 'SPCEP Ialomita', '2020-01-22', 'SYSTEM'),

-- Personal Resurse Umane
('EMP024', '4456789012346', 'Radulescu', 'Catalin', '1986-12-03', 'Calarasi', '0756789014', 'catalin.radulescu@valyanmed.ro', 'Str. Independentei nr. 22', 'Calarasi', 'Calarasi', 'Casatorit/a', 'Specialist HR', 'Resurse Umane', 'CL', '445679', 'SPCEP Calarasi', '2021-11-18', 'SYSTEM'),
('EMP025', '5567890123457', 'Constantinescu', 'Alina', '1991-02-16', 'Giurgiu', '0767890125', 'alina.constantinescu@valyanmed.ro', 'Str. Mihail Kogalniceanu nr. 8', 'Giurgiu', 'Giurgiu', 'Necasatorit/a', 'Recruitment Specialist', 'Resurse Umane', 'GR', '556790', 'SPCEP Giurgiu', '2022-06-05', 'SYSTEM'),

-- Personal Diverse
('EMP026', '6678901234568', 'Badea', 'Sorin', '1988-07-09', 'Tulcea', '0778901236', 'sorin.badea@valyanmed.ro', 'Bd. Babadag nr. 19', 'Tulcea', 'Tulcea', 'Divortat/a', 'Sofer', 'Transport', 'TL', '667891', 'SPCEP Tulcea', '2021-12-12', 'SYSTEM'),
('EMP027', '7789012345679', 'Dumitrescu', 'Gabriela', '1985-01-26', 'Alexandria', '0789012347', 'gabriela.dumitrescu@valyanmed.ro', 'Str. Dunarii nr. 17', 'Teleorman', 'Alexandria', 'Casatorit/a', 'Operator Call Center', 'Call Center', 'TR', '778902', 'SPCEP Teleorman', '2020-08-07', 'SYSTEM'),
('EMP028', '8890123456790', 'Enache', 'Lucian', '1990-04-20', 'Rosiorii de Vede', '0790123458', 'lucian.enache@valyanmed.ro', 'Str. Victoriei nr. 11', 'Teleorman', 'Rosiorii de Vede', 'Necasatorit/a', 'Specialist Comunicare', 'Marketing', 'TR', '889013', 'SPCEP Teleorman', '2022-03-22', 'SYSTEM'),
('EMP029', '9901234567891', 'Georgescu', 'Carmen', '1987-09-18', 'Dragasani', '0701234569', 'carmen.georgescu@valyanmed.ro', 'Str. Traian nr. 24', 'Valcea', 'Dragasani', 'Casatorit/a', 'Arhivar', 'Administratie', 'VL', '990124', 'SPCEP Valcea', '2019-07-14', 'SYSTEM'),
('EMP030', '0012345678902', 'Manole', 'Daniel', '1984-11-11', 'Targoviste', '0712345670', 'daniel.manole@valyanmed.ro', 'Bd. Carol I nr. 50', 'Dambovita', 'Targoviste', 'Vaduv/a', 'Consilier Juridic', 'Juridic', 'DB', '001235', 'SPCEP Dambovita', '2018-05-28', 'SYSTEM');