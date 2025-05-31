[<AutoOpen>]
module CleanupUserProfile.Operators

let (<&&>) a b =
    And [
        a
        b
    ]

let (<||>) a b =
    Or [
        a
        b
    ]
