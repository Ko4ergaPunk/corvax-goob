# Poggers examine system

examine-name = Это же [bold]{ $name }[/bold]!
examine-can-see = Осмотрев { OBJECT($ent) }, вы можете увидеть:
examine-can-see-nothing = { CAPITALIZE(SUBJECT($ent)) } полностью без ничего!
id-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } ремне.
examine-border-line = ═════════════════════
examine-present-tex = Это [enttex id="{ $id }" size={ $size }] [bold]{ $name }[/bold]!
examine-present = Это [bold]{ $name }[/bold]!
examine-present-line = ═══
head-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } голове.
eyes-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } глазах.
mask-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } лице.
neck-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } шее.
ears-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } ушах.
jumpsuit-examine = - [bold]{ $item }[/bold], надетый на н{ POSS-ADJ($ent) }.
outer-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } теле.
suitstorage-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } плече.
back-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } спине.
gloves-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } руках.
belt-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } поясе.
shoes-examine = - [bold]{ $item }[/bold] на { POSS-ADJ($ent) } ногах.

# Selfaware version

id-card-examine-full = • { CAPITALIZE(POSS-ADJ($wearer)) } ID: [bold]{ $nameAndJob }[/bold].
examine-name-selfaware = Это вы, [bold]{ $name }[/bold]!
examine-can-see-selfaware = Осмотрев себя, вы можете увидеть:
examine-can-see-nothing-selfaware = На вас вообще ничего нет!
id-examine-selfaware = - [bold]{ $item }[/bold] на вашем поясе.
head-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашей голове.
eyes-examine-selfaware = - [bold]{ $item }[/bold] на ваших глазах.
mask-examine-selfaware = - [bold]{ $item }[/bold] на вашем лице.
neck-examine-selfaware = - [bold]{ $item }[/bold] на вашей шее.
ears-examine-selfaware = - [bold]{ $item }[/bold] на ваших ушах.
jumpsuit-examine-selfaware = - [bold]{ $item }[/bold] надетый на вас.
outer-examine-selfaware = - [bold]{ $item }[/bold] на вашем теле.
suitstorage-examine-selfaware = - [bold]{ $item }[/bold] на вашем плече.
back-examine-selfaware = - [bold]{ $item }[/bold] на вашей спине.
gloves-examine-selfaware = - [bold]{ $item }[/bold] на ваших руках.
belt-examine-selfaware = - [bold]{ $item }[/bold] на вашем поясе.
shoes-examine-selfaware = - [bold]{ $item }[/bold] на ваших ногах.

# Selfaware examine

comp-hands-examine-empty-selfaware = Вы ничего не держите.
comp-hands-examine-selfaware = Вы держите { $items }.
humanoid-appearance-component-examine-selfaware = Вы - { $species } { $age }.
