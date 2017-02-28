<?php
include_once "../dbinfo.inc";
include_once "Player.php";

    $DB_SERVER = DB_SERVER;
    $DB_NAME = DB_DATABASE;
    $DB_USER = DB_USERNAME;
    $DB_PASS = DB_PASSWORD;

	$searchTerm = $_GET['search'];

	try {
        $conn = new PDO("mysql:host=" . $this->DB_SERVER . ";dbname=" . $this->DB_NAME, $this->DB_USER, $this->DB_PASS);
        // set the PDO error mode to exception
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        $stmt = self::$conn->prepare("SELECT * FROM `2015-2016` WHERE Name=:name");
        $name = strtolower(filter_var($searchTerm, FILTER_SANITIZE_STRING));
        $stmt->bindParam(':name', $searchTerm);
        $stmt->execute();
        $stmt->setFetchMode(PDO::FETCH_CLASS | PDO::FETCH_PROPS_LATE,
            'Player',
            array('name', 'team', 'gp', 'min', 'fg_m', 'fg_a', 'fg_pct',
                'three_pt_m', 'three_pt_a', 'three_pt_pct', 'ft_m', 'ft_a',
                'ft_pct', 'reb_off', 'reb_def', 'reb_tot', 'ast', 'to', 'stl',
                'blk', 'pf', 'ppg'));
        $results = $stmt->fetchAll();

        return json_encode($results);

        $stmt->closeCursor();
        $conn = null;

    } catch (PDOException $e) {
        return "error retrieving nba player stats";
    }
?>